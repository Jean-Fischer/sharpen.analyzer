using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class LambdaParameterModifiersWithoutTypesSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (!CSharpLanguageVersion.IsCSharp14OrAbove(semanticModel.Compilation))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "csharp_version",
                "C# 14 or above is required.");

        var root = syntaxTree.GetRoot(cancellationToken);
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        var parameterList = node as ParameterListSyntax ?? node.FirstAncestorOrSelf<ParameterListSyntax>();
        if (parameterList is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "node_not_found",
                "Expected a lambda parameter list.");

        if (parameterList.Parent is not ParenthesizedLambdaExpressionSyntax lambda)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not_parenthesized_lambda",
                "Expected a parenthesized lambda.");

        // Ensure the lambda is target-typed to a delegate.
        var convertedType = semanticModel.GetTypeInfo(lambda, cancellationToken).ConvertedType;
        if (convertedType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not_target_typed",
                "Lambda is not target-typed to a delegate.");

        // Ensure we have at least one parameter with both modifiers and an explicit type.
        var hasCandidate = false;
        foreach (var parameter in parameterList.Parameters)
            if (parameter.Modifiers.Count > 0 && parameter.Type is not null)
            {
                hasCandidate = true;
                break;
            }

        if (!hasCandidate)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no_candidate",
                "No parameter has both modifiers and an explicit type.");

        // Conservative: we do not attempt to simulate overload resolution changes here.
        // The analyzer only reports in already target-typed contexts.
        return FixProviderSafetyResult.Safe();
    }
}