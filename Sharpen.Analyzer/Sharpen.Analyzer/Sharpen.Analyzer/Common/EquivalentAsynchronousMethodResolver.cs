using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Common;

public static class EquivalentAsynchronousMethodResolver
{
    public static IMethodSymbol? ResolveAsyncEquivalent(
        InvocationExpressionSyntax? invocation,
        SemanticModel? semanticModel)
    {
        if (invocation?.Expression == null) return null;

        if (semanticModel?.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method) return null;

        // Mirror the finder behavior: ignore known methods.
        if (EquivalentAsynchronousMethodMetadata.IsIgnoredMethod(method)) return null;

        // Mirror the finder behavior: ignore lambdas/anonymous methods.
        if (invocation.IsWithinLambdaOrAnonymousMethod()) return null;

        // Mirror the finder behavior: ignore invocations within the containing type.
        if (MethodIsInvokedWithinItsContainingType(invocation, semanticModel, method)) return null;

        // Candidate search strategy: check containing type first, then receiver type (if different).
        var asyncEquivalent = EquivalentAsynchronousMethodLookup.ResolveAsynchronousEquivalent(
            semanticModel,
            method.ContainingType,
            method,
            invocation);
        if (asyncEquivalent != null) return asyncEquivalent;

        var calledOnType = GetCalledOnType(invocation, semanticModel);
        return SymbolEqualityComparer.Default.Equals(calledOnType, method.ContainingType)
            ? null
            : EquivalentAsynchronousMethodLookup.ResolveAsynchronousEquivalent(
                semanticModel,
                calledOnType,
                method,
                invocation);
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

    private static INamedTypeSymbol? GetCalledOnType(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess)) return null;

        return semanticModel.GetTypeInfo(memberAccess.Expression).Type as INamedTypeSymbol;
    }
}
