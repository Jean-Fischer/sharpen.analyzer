using System;
using System.Collections.Generic;
using System.Linq;
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
        if (diagnostic is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var parameter = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ParameterSyntax>();
        if (parameter is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "parameter-not-found");

        var method = parameter.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
        if (method is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "method-not-found");

        var methodSymbol = semanticModel.GetDeclaredSymbol(method, cancellationToken) as IMethodSymbol;
        if (methodSymbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "method-symbol-null");

        // Only allow non-public APIs.
        if (methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "public-or-protected");

        var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken) as IParameterSymbol;
        if (parameterSymbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "parameter-symbol-null");

        if (!parameterSymbol.IsParams)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-params");

        if (parameterSymbol.Type is not IArrayTypeSymbol arrayType || arrayType.Rank != 1)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-1d-array");

        // Target type must exist.
        var readOnlySpanType = semanticModel.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
        if (readOnlySpanType is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "readonlyspan-missing");

        // Body must not use array-only semantics.
        if (method.Body is null && method.ExpressionBody is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "no-body");

        var forbidden = FindForbiddenArraySemantics(method, parameterSymbol, semanticModel, cancellationToken);
        if (forbidden.Count > 0)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "array-semantics", message: string.Join(", ", forbidden));

        // NOTE: We do not attempt to prove "no external call sites" here because the safety checker
        // runs per-document. The fix provider updates in-solution references; external callers are
        // out of scope and this is why we restrict to non-public APIs.

        return FixProviderSafetyResult.Safe();
    }

    private static HashSet<string> FindForbiddenArraySemantics(
        BaseMethodDeclarationSyntax method,
        IParameterSymbol parameterSymbol,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var forbidden = new HashSet<string>(StringComparer.Ordinal);

        IEnumerable<SyntaxNode> nodes = method.Body?.DescendantNodes() ?? method.ExpressionBody!.DescendantNodes();

        foreach (var node in nodes)
        {
            // values.Length
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                if (IsParameterReference(memberAccess.Expression, parameterSymbol, semanticModel, ct))
                {
                    var name = memberAccess.Name.Identifier.ValueText;
                    if (name is "Length" or "LongLength" or "Rank")
                        forbidden.Add($"member:{name}");
                }
            }

            // values[i]
            if (node is ElementAccessExpressionSyntax elementAccess)
            {
                if (IsParameterReference(elementAccess.Expression, parameterSymbol, semanticModel, ct))
                    forbidden.Add("indexing");
            }

            // values.GetLength(...)
            if (node is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax invokedMember)
                {
                    if (IsParameterReference(invokedMember.Expression, parameterSymbol, semanticModel, ct))
                    {
                        var name = invokedMember.Name.Identifier.ValueText;
                        if (name is "GetLength" or "GetLowerBound" or "GetUpperBound" or "CopyTo" or "Clone")
                            forbidden.Add($"call:{name}");
                    }

                    // Array.*(values, ...)
                    var invokedSymbol = semanticModel.GetSymbolInfo(invocation, ct).Symbol as IMethodSymbol;
                    if (invokedSymbol?.ContainingType?.ToDisplayString() == "System.Array")
                    {
                        foreach (var arg in invocation.ArgumentList.Arguments)
                        {
                            if (IsParameterReference(arg.Expression, parameterSymbol, semanticModel, ct))
                            {
                                forbidden.Add($"System.Array:{invokedSymbol.Name}");
                                break;
                            }
                        }
                    }
                }
            }
        }

        return forbidden;
    }

    private static bool IsParameterReference(ExpressionSyntax expression, IParameterSymbol parameterSymbol, SemanticModel semanticModel, CancellationToken ct)
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
