using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Common;
// TODO-IG: Refactor this. The class heavily violates the single responsibility principle.
//          It does two heavy things at the moment, searches for the equivalent async methods,
//          but also checks the "environment" to see if it fits requirements of just a one
//          specific client. Bad.

/// <summary>
///
/// </summary>
/// <remarks>
///     We can have different finders. Hardcoded one, one based on dependencies etc.
///     This base class encapsulates the common and exact search logic.
///     The derived classes are responsible for the heuristic part of the search.
/// </remarks>
internal abstract class EquivalentAsynchronousMethodFinder
{
    public enum CallerAsyncStatus
    {
        CallerMustBeAsync,
        CallerMustNotBeAsync
    }

    public enum CallerYieldingStatus
    {
        Irrelevant,
        CallerMustYield,
        CallerMustNotYield
    }

    /// <summary>
    ///     Returns true if an equivalent asynchronous method of the method used in
    ///     the <paramref name="invocation" /> exists and is at the same time a
    ///     potential candidate to be used exactly in that particular <paramref name="invocation" />.
    /// </summary>
    /// <remarks>
    ///     This method does not only check if an equivalent asynchronous method
    ///     exists. In addition, it runs additional checks to see if it make sense
    ///     to call such existing asynchronous equivalent in the particular <paramref name="invocation" />.
    ///     For example, the suggestion to await asynchronous equivalent
    ///     makes sense only if the enclosing method
    ///     within which the invocation happens can be turned into async method.
    ///     For example, if the enclosing method is an interface implementation or an override
    ///     method of an interface or base class that cannot be changed, than it cannot be
    ///     turned into async method and thus the suggestion makes no sense.
    /// </remarks>
    public bool EquivalentAsynchronousCandidateExistsFor(InvocationExpressionSyntax invocation,
        SemanticModel semanticModel, CallerAsyncStatus callerAsyncStatus, CallerYieldingStatus callerYieldingStatus)
    {
        if (!InvokedMethodPotentiallyHasAsynchronousEquivalent(invocation)) return false;

        if (!TryGetInvokedMethod(semanticModel, invocation, out var method)) return false;

        if (EquivalentAsynchronousMethodMetadata.IsIgnoredMethod(method)) return false;

        // So far we do not suggest turning lambdas and anonymous methods
        // into async. So we will at the moment just ignore that case.
        // TODO: Support suggestion for lambdas and anonymous methods.
        if (invocation.IsWithinLambdaOrAnonymousMethod()) return false;

        // If type authors invoke the synchronous method
        // within the implementation of its containing type
        // we assume that they exactly know what they are doing.
        // They for sure want to call exactly that method on
        // that particular place in code. We are 100% sure that
        // they do not want to call its async equivalent.
        if (MethodIsInvokedWithinItsContainingType(invocation, semanticModel, method)) return false;

        if (!MethodIsInvokedWithinACallerNodeThatCanBeMarkedAsAsync(invocation)) return false;

        if (!TryGetEnclosingLocalFunctionOrMethod(invocation, semanticModel, out var callerSymbol, out var callerSyntaxNode))
            return false;

        if (!CallerMatchesAsyncStatus(callerSymbol, callerAsyncStatus, semanticModel)) return false;

        if (!CallerMatchesYieldingStatus(callerSyntaxNode, callerYieldingStatus)) return false;

        return ContainsEquivalentOnContainingType(semanticModel, method, invocation)
               || ContainsEquivalentOnCalledOnType(semanticModel, method, invocation);
    }

    protected abstract bool InvokedMethodPotentiallyHasAsynchronousEquivalent(InvocationExpressionSyntax invocation);

    private static bool TryGetInvokedMethod(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        out IMethodSymbol method)
    {
        method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        return method is not null;
    }

    private static bool MethodIsInvokedWithinACallerNodeThatCanBeMarkedAsAsync(InvocationExpressionSyntax invocation)
    {
        return invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null;
    }

    private static bool TryGetEnclosingLocalFunctionOrMethod(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out IMethodSymbol callerSymbol,
        out SyntaxNode callerSyntaxNode)
    {
        var enclosingLocalFunction = invocation.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (enclosingLocalFunction != null)
        {
            callerSymbol = semanticModel.GetDeclaredSymbol(enclosingLocalFunction)!;
            callerSyntaxNode = enclosingLocalFunction;
            return callerSymbol is not null;
        }

        var enclosingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (enclosingMethod != null)
        {
            callerSymbol = semanticModel.GetDeclaredSymbol(enclosingMethod)!;
            callerSyntaxNode = enclosingMethod;
            return callerSymbol is not null;
        }

        callerSymbol = null!;
        callerSyntaxNode = null!;
        return false;
    }

    private static bool CallerMatchesAsyncStatus(
        IMethodSymbol callerSymbol,
        CallerAsyncStatus callerAsyncStatus,
        SemanticModel semanticModel)
    {
        return callerAsyncStatus switch
        {
            CallerAsyncStatus.CallerMustNotBeAsync => !callerSymbol.IsAsync && CallerCanBeMadeAsync(callerSymbol, semanticModel),
            CallerAsyncStatus.CallerMustBeAsync => callerSymbol.IsAsync,
            _ => false
        };
    }

    private static bool CallerMatchesYieldingStatus(SyntaxNode callerSyntaxNode, CallerYieldingStatus callerYieldingStatus)
    {
        if (callerYieldingStatus == CallerYieldingStatus.Irrelevant)
            return true;

        var callerYields = callerSyntaxNode.Yields();
        return callerYieldingStatus switch
        {
            CallerYieldingStatus.CallerMustYield => callerYields,
            CallerYieldingStatus.CallerMustNotYield => !callerYields,
            _ => true
        };
    }

    private static bool CallerCanBeMadeAsync(IMethodSymbol callerSymbol, SemanticModel semanticModel)
    {
        if (callerSymbol.MethodKind == MethodKind.LocalFunction)
            return true;

        return CallerMethodDoesNotOverrideNonChangeableBaseClassMethod(callerSymbol)
               && CallerMethodDoesNotImplementNonChangeableInterfaceMethod(callerSymbol)
               && CallerMethodDoesNotAlreadyHaveAsynchronousEquivalent(callerSymbol, semanticModel);
    }

    private static bool CallerMethodDoesNotOverrideNonChangeableBaseClassMethod(IMethodSymbol callerSymbol)
    {
        if (!callerSymbol.IsOverride)
            return true;

        return callerSymbol.OverriddenMethod?.ContainingType?.Locations.All(location => location.IsInSource) == true;
    }

    private static bool CallerMethodDoesNotImplementNonChangeableInterfaceMethod(IMethodSymbol callerSymbol)
    {
        return callerSymbol.GetImplementedInterfaceMethods()
            .All(interfaceMethod => interfaceMethod.ContainingType?.Locations.All(location => location.IsInSource) == true);
    }

    private static bool CallerMethodDoesNotAlreadyHaveAsynchronousEquivalent(
        IMethodSymbol callerSymbol,
        SemanticModel semanticModel)
    {
        return !EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
            semanticModel,
            callerSymbol.ContainingType,
            callerSymbol);
    }

    private static bool MethodIsInvokedWithinItsContainingType(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IMethodSymbol method)
    {
        var invokedInType = invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (invokedInType == null) return true;

        return SymbolEqualityComparer.Default.Equals(method.ContainingType, semanticModel.GetDeclaredSymbol(invokedInType));
    }

    private static bool ContainsEquivalentOnContainingType(
        SemanticModel semanticModel,
        IMethodSymbol method,
        InvocationExpressionSyntax invocation)
    {
        return EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
            semanticModel,
            method.ContainingType,
            method,
            invocation);
    }

    private static bool ContainsEquivalentOnCalledOnType(
        SemanticModel semanticModel,
        IMethodSymbol method,
        InvocationExpressionSyntax invocation)
    {
        var calledOnType = GetCalledOnType(invocation, semanticModel);
        return calledOnType != null
               && !SymbolEqualityComparer.Default.Equals(calledOnType, method.ContainingType)
               && EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
                   semanticModel,
                   calledOnType,
                   method,
                   invocation);
    }

    private static INamedTypeSymbol? GetCalledOnType(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return null;

        return semanticModel.GetTypeInfo(memberAccess.Expression).Type as INamedTypeSymbol;
    }
}
