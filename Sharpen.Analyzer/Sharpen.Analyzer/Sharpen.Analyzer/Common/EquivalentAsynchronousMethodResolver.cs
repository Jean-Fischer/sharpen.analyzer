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
        if (invocation is null || semanticModel is null)
            return null;

        if (!TryGetInvokedMethod(invocation, semanticModel, out var method) || method is null)
            return null;

        if (ShouldIgnoreInvocation(invocation, semanticModel, method))
            return null;

        return ResolveOnContainingType(invocation, semanticModel, method)
               ?? ResolveOnCalledOnType(invocation, semanticModel, method);
    }

    private static bool TryGetInvokedMethod(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        out IMethodSymbol? method)
    {
        method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        return method is not null;
    }

    private static bool ShouldIgnoreInvocation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IMethodSymbol method)
    {
        return EquivalentAsynchronousMethodMetadata.IsIgnoredMethod(method)
               || invocation.IsWithinLambdaOrAnonymousMethod()
               || MethodIsInvokedWithinItsContainingType(invocation, semanticModel, method);
    }

    private static IMethodSymbol? ResolveOnContainingType(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IMethodSymbol method)
    {
        return EquivalentAsynchronousMethodLookup.ResolveAsynchronousEquivalent(
            semanticModel,
            method.ContainingType,
            method,
            invocation);
    }

    private static IMethodSymbol? ResolveOnCalledOnType(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        IMethodSymbol method)
    {
        var calledOnType = GetCalledOnType(invocation, semanticModel);
        if (SymbolEqualityComparer.Default.Equals(calledOnType, method.ContainingType))
            return null;

        return EquivalentAsynchronousMethodLookup.ResolveAsynchronousEquivalent(
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
