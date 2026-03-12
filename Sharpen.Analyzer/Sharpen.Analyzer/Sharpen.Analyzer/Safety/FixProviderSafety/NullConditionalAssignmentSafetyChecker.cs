using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var ifStatement = node.FirstAncestorOrSelf<IfStatementSyntax>() ?? node as IfStatementSyntax;
        if (ifStatement is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "if-not-found");

        if (!Analyzers.CSharp14.NullConditionalAssignmentPattern.TryGetNullCheckedExpression(ifStatement.Condition, out var checkedExpression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "condition-not-null-check");

        if (!Analyzers.CSharp14.NullConditionalAssignmentPattern.TryGetSingleStatement(ifStatement.Statement, out var singleStatement))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-single-statement");

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-assignment-statement");

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-simple-assignment");

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "lhs-not-member-access");

        if (!SyntaxFactory.AreEquivalent(
                Analyzers.CSharp14.NullConditionalAssignmentPattern.StripParentheses(checkedExpression),
                Analyzers.CSharp14.NullConditionalAssignmentPattern.StripParentheses(memberAccess.Expression)))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "receiver-mismatch");

        return FixProviderSafetyResult.Safe();
    }
}
