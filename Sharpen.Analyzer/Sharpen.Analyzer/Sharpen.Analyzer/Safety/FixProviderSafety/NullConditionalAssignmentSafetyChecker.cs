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

        if (!TryGetNullCheckedExpression(ifStatement.Condition, out var checkedExpression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "condition-not-null-check");

        if (!TryGetSingleStatement(ifStatement.Statement, out var singleStatement))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-single-statement");

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-assignment-statement");

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "not-simple-assignment");

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "lhs-not-member-access");

        if (!AreEquivalentIgnoringParentheses(checkedExpression, memberAccess.Expression))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, reasonId: "receiver-mismatch");

        return FixProviderSafetyResult.Safe();
    }

    private static bool TryGetNullCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax checkedExpression)
    {
        checkedExpression = null!;

        if (condition is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (IsNullLiteral(binary.Right))
            {
                checkedExpression = binary.Left;
                return true;
            }

            if (IsNullLiteral(binary.Left))
            {
                checkedExpression = binary.Right;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetSingleStatement(StatementSyntax statement, out StatementSyntax singleStatement)
    {
        if (statement is BlockSyntax block)
        {
            if (block.Statements.Count == 1)
            {
                singleStatement = block.Statements[0];
                return true;
            }

            singleStatement = null!;
            return false;
        }

        singleStatement = statement;
        return true;
    }

    private static bool IsNullLiteral(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool AreEquivalentIgnoringParentheses(ExpressionSyntax left, ExpressionSyntax right)
    {
        left = StripParentheses(left);
        right = StripParentheses(right);

        return SyntaxFactory.AreEquivalent(left, right);
    }

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
