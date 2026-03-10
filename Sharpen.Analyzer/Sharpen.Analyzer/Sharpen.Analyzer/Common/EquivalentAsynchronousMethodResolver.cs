using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Engine.Extensions;

namespace Sharpen.Engine.SharpenSuggestions.Common.AsyncAwaitAndAsyncStreams
{
    internal static class EquivalentAsynchronousMethodResolver
    {
        public static IMethodSymbol ResolveAsyncEquivalent(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel)
        {
            if (invocation?.Expression == null) return null;
            if (semanticModel == null) return null;

            if (!(semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method)) return null;

            // Mirror the finder behavior: ignore known methods.
            if (IsIgnoredMethod(method)) return null;

            // Mirror the finder behavior: ignore lambdas/anonymous methods.
            if (invocation.IsWithinLambdaOrAnonymousMethod()) return null;

            // Mirror the finder behavior: ignore invocations within the containing type.
            if (MethodIsInvokedWithinItsContainingType(invocation, semanticModel, method)) return null;

            // Candidate search strategy: check containing type first, then receiver type (if different).
            var asyncEquivalent = FindAsyncEquivalentOnType(semanticModel, method.ContainingType, method, invocation);
            if (asyncEquivalent != null) return asyncEquivalent;

            var calledOnType = GetCalledOnType(invocation, semanticModel);
            if (calledOnType == null || SymbolEqualityComparer.Default.Equals(calledOnType, method.ContainingType)) return null;

            return FindAsyncEquivalentOnType(semanticModel, calledOnType, method, invocation);
        }

        private static bool IsIgnoredMethod(IMethodSymbol method)
        {
            // Keep in sync with EquivalentAsynchronousMethodFinder.KnownMethodsToIgnore.
            // (We intentionally duplicate the list here because the finder keeps it private.)
            return method.ContainingType?.Name == "DbSet" &&
                   method.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore" &&
                   (method.Name == "Add" || method.Name == "AddRange");
        }

        private static IMethodSymbol FindAsyncEquivalentOnType(
            SemanticModel semanticModel,
            INamedTypeSymbol type,
            IMethodSymbol syncMethod,
            InvocationExpressionSyntax invocation)
        {
            if (type == null) return null;
            if (syncMethod == null) return null;

            var asyncName = syncMethod.Name + "Async";

            // Use LookupSymbols at the invocation position to match the finder behavior
            // (partial types + extension methods + reduced extension methods).
            var candidates = semanticModel
                .LookupSymbols(invocation.Expression?.SpanStart ?? 0, type, asyncName, includeReducedExtensionMethods: true)
                .OfType<IMethodSymbol>();

            return candidates.FirstOrDefault(candidate => IsAsynchronousEquivalent(candidate, syncMethod));
        }

        private static bool IsAsynchronousEquivalent(IMethodSymbol potentialEquivalent, IMethodSymbol method)
        {
            // Copied from EquivalentAsynchronousMethodFinder.TypeContainsAsynchronousEquivalentOf(...)
            // but returns the chosen symbol instead of bool.

            if (potentialEquivalent == null) return false;

            // We insist that the async method returns an awaitable object.
            if (potentialEquivalent.ReturnsVoid) return false;
            if (potentialEquivalent.ReturnType == null) return false;

            // If the method returns void its async equivalent must return
            // any of the known awaitable types that can be void equivalents.
            if (method.ReturnsVoid)
            {
                if (!EquivalentAsynchronousMethodFinder.KnownAwaitableTypes.Any(awaitableType =>
                        awaitableType.IsVoidEquivalent &&
                        awaitableType.RepresentsType(potentialEquivalent.ReturnType)))
                    return false;
            }
            else
            {
                if (method.ReturnType == null) return false;

                if (!(potentialEquivalent.ReturnType is INamedTypeSymbol potentialEquivalentReturnType))
                    return false;

                if (potentialEquivalentReturnType.Arity != 1)
                    return false;

                var returnedKnownAwaitableType = EquivalentAsynchronousMethodFinder.KnownAwaitableTypes
                    .FirstOrDefault(awaitableType => awaitableType.RepresentsType(potentialEquivalentReturnType.ConstructedFrom));
                if (returnedKnownAwaitableType == null) return false;

                if (returnedKnownAwaitableType.WrapsReturnType())
                {
                    if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, potentialEquivalentReturnType.TypeArguments[0]))
                        return false;
                }
                else
                {
                    if (!(method.ReturnType is INamedTypeSymbol methodReturnType && methodReturnType.Arity == 1))
                        return false;

                    if (!SymbolEqualityComparer.Default.Equals(methodReturnType.TypeArguments[0], potentialEquivalentReturnType.TypeArguments[0]))
                        return false;
                }
            }

            // Parameter compatibility: same type + name, optional trailing CancellationToken.
            int numberOfParameters = method.Parameters.Length;
            if (!(potentialEquivalent.Parameters.Length == numberOfParameters ||
                  potentialEquivalent.Parameters.Length == numberOfParameters + 1))
                return false;

            for (int i = 0; i < numberOfParameters; i++)
            {
                if (method.Parameters[i].Type == null)
                    return false;
                if (!SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, potentialEquivalent.Parameters[i].Type))
                    return false;
                if (method.Parameters[i].Name != potentialEquivalent.Parameters[i].Name)
                    return false;
            }

            if (potentialEquivalent.Parameters.Length == numberOfParameters + 1)
            {
                if (!potentialEquivalent.Parameters[numberOfParameters].Type
                    .FullNameIsEqualTo("System.Threading", "CancellationToken"))
                    return false;
            }

            return true;
        }

        private static bool MethodIsInvokedWithinItsContainingType(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            IMethodSymbol method)
        {
            var invokedInType = invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            // If syntax tree is unexpected, be conservative and treat as within containing type.
            if (invokedInType == null) return true;

            return SymbolEqualityComparer.Default.Equals(method.ContainingType, semanticModel.GetDeclaredSymbol(invokedInType));
        }

        private static INamedTypeSymbol GetCalledOnType(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
        {
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess)) return null;
            if (memberAccess.Expression == null) return null;

            return semanticModel.GetTypeInfo(memberAccess.Expression).Type as INamedTypeSymbol;
        }
    }
}
