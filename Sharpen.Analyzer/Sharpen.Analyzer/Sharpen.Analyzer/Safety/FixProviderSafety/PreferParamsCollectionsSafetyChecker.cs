using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class PreferParamsCollectionsSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetTargetSymbols(
                syntaxTree,
                semanticModel,
                diagnostic,
                cancellationToken,
                out var method,
                out var methodSymbol,
                out var parameterSymbol,
                out var failureCode))
        {
            return Unsafe(failureCode);
        }

        if (IsPublicOrProtectedApi(methodSymbol))
            return Unsafe("public-or-protected");

        if (!IsSupportedParamsArray(parameterSymbol))
            return Unsafe("not-1d-array");

        if (!CompilationSupportsReadOnlySpan(semanticModel.Compilation))
            return Unsafe("readonlyspan-missing");

        if (!HasMethodBody(method))
            return Unsafe("no-body");

        var forbidden = FindForbiddenArraySemantics(method, parameterSymbol, semanticModel, cancellationToken);
        if (forbidden.Count > 0)
        {
            return Unsafe("array-semantics", string.Join(", ", forbidden));
        }

        // NOTE: We do not attempt to prove "no external call sites" here because the safety checker
        // runs per-document. The fix provider updates in-solution references; external callers are
        // out of scope and this is why we restrict to non-public APIs.

        return FixProviderSafetyResult.Safe();
    }

    private static bool TryGetTargetSymbols(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken,
        out BaseMethodDeclarationSyntax method,
        out IMethodSymbol methodSymbol,
        out IParameterSymbol parameterSymbol,
        out string failureCode)
    {
        method = null!;
        methodSymbol = null!;
        parameterSymbol = null!;

        if (diagnostic is null)
        {
            failureCode = "diagnostic-null";
            return false;
        }

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
        {
            failureCode = "not-csharp";
            return false;
        }

        var parameter = syntaxTree.GetRoot(cancellationToken)
            .FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ParameterSyntax>();
        if (parameter is null)
        {
            failureCode = "parameter-not-found";
            return false;
        }

        method = parameter.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()!;
        if (method is null)
        {
            failureCode = "method-not-found";
            return false;
        }

        methodSymbol = semanticModel.GetDeclaredSymbol(method, cancellationToken)!;
        if (methodSymbol is null)
        {
            failureCode = "method-symbol-null";
            return false;
        }

        parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken)!;
        if (parameterSymbol is null)
        {
            failureCode = "parameter-symbol-null";
            return false;
        }

        if (!parameterSymbol.IsParams)
        {
            failureCode = "not-params";
            return false;
        }

        failureCode = string.Empty;
        return true;
    }

    private static bool IsPublicOrProtectedApi(IMethodSymbol methodSymbol)
    {
        return methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
            or Accessibility.ProtectedOrInternal;
    }

    private static bool IsSupportedParamsArray(IParameterSymbol parameterSymbol)
    {
        return parameterSymbol.Type is IArrayTypeSymbol { Rank: 1 };
    }

    private static bool CompilationSupportsReadOnlySpan(Compilation compilation)
    {
        return compilation.GetTypeByMetadataName("System.ReadOnlySpan`1") is not null;
    }

    private static bool HasMethodBody(BaseMethodDeclarationSyntax method)
    {
        return method.Body is not null || method.ExpressionBody is not null;
    }

    private static HashSet<string> FindForbiddenArraySemantics(
        BaseMethodDeclarationSyntax method,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var forbidden = new HashSet<string>(StringComparer.Ordinal);

        var nodes = method.Body?.DescendantNodes() ?? method.ExpressionBody!.DescendantNodes();

        foreach (var node in nodes)
        {
            AddForbiddenMemberAccess(node, parameterSymbol, semanticModel, ct, forbidden);
            AddForbiddenIndexAccess(node, parameterSymbol, semanticModel, ct, forbidden);
            AddForbiddenInvocation(node, parameterSymbol, semanticModel, ct, forbidden);
        }

        return forbidden;
    }

    private static void AddForbiddenMemberAccess(
        SyntaxNode node,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<string> forbidden)
    {
        if (node is not MemberAccessExpressionSyntax memberAccess
            || !IsParameterReference(memberAccess.Expression, parameterSymbol, semanticModel, cancellationToken))
        {
            return;
        }

        var name = memberAccess.Name.Identifier.ValueText;
        if (name is "Length" or "LongLength" or "Rank")
            forbidden.Add($"member:{name}");
    }

    private static void AddForbiddenIndexAccess(
        SyntaxNode node,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<string> forbidden)
    {
        if (node is ElementAccessExpressionSyntax elementAccess
            && IsParameterReference(elementAccess.Expression, parameterSymbol, semanticModel, cancellationToken))
        {
            forbidden.Add("indexing");
        }
    }

    private static void AddForbiddenInvocation(
        SyntaxNode node,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<string> forbidden)
    {
        if (node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax invokedMember } invocation)
            return;

        AddForbiddenArrayInstanceCall(invokedMember, parameterSymbol, semanticModel, cancellationToken, forbidden);
        AddForbiddenArrayStaticCall(invocation, parameterSymbol, semanticModel, cancellationToken, forbidden);
    }

    private static void AddForbiddenArrayInstanceCall(
        MemberAccessExpressionSyntax invokedMember,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<string> forbidden)
    {
        if (!IsParameterReference(invokedMember.Expression, parameterSymbol, semanticModel, cancellationToken))
            return;

        var name = invokedMember.Name.Identifier.ValueText;
        if (name is "GetLength" or "GetLowerBound" or "GetUpperBound" or "CopyTo" or "Clone")
            forbidden.Add($"call:{name}");
    }

    private static void AddForbiddenArrayStaticCall(
        InvocationExpressionSyntax invocation,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        ISet<string> forbidden)
    {
        var invokedSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
        if (invokedSymbol?.ContainingType?.ToDisplayString() != "System.Array")
            return;

        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (!IsParameterReference(arg.Expression, parameterSymbol, semanticModel, cancellationToken))
                continue;

            forbidden.Add($"System.Array:{invokedSymbol.Name}");
            break;
        }
    }

    private static FixProviderSafetyResult Unsafe(string code, string? details = null)
    {
        return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, code, details);
    }

    private static bool IsParameterReference(ExpressionSyntax expression, IParameterSymbol parameterSymbol,
        SemanticModel semanticModel, CancellationToken ct)
    {
        expression = expression switch
        {
            ParenthesizedExpressionSyntax p => p.Expression,
            _ => expression
        };

        if (expression is IdentifierNameSyntax)
        {
            var symbol = semanticModel.GetSymbolInfo(expression, ct).Symbol;
            return SymbolEqualityComparer.Default.Equals(symbol, parameterSymbol);
        }

        return false;
    }
}
