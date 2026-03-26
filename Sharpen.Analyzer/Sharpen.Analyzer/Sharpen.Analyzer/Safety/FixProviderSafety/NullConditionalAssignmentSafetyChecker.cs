using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Analyzers.CSharp14;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class NullConditionalAssignmentSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (diagnostic is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var ifStatement = node.FirstAncestorOrSelf<IfStatementSyntax>() ?? node as IfStatementSyntax;
        if (ifStatement is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "if-not-found");

        if (!NullConditionalAssignmentPattern.TryGetNullCheckedExpression(ifStatement.Condition,
                out var checkedExpression))
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "condition-not-null-check");
        }

        if (!NullConditionalAssignmentPattern.TryGetSingleStatement(ifStatement.Statement, out var singleStatement))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-single-statement");

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-assignment-statement");

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-simple-assignment");

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "lhs-not-member-access");

        if (!SyntaxFactory.AreEquivalent(
                NullConditionalAssignmentPattern.StripParentheses(checkedExpression),
                NullConditionalAssignmentPattern.StripParentheses(memberAccess.Expression)))
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "receiver-mismatch");
        }

        return FixProviderSafetyResult.Safe();
    }
}