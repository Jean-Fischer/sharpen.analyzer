using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

public static class NullConditionalAssignmentPattern
{
    public static bool TryMatch(
        IfStatementSyntax ifStatement,
        out ExpressionSyntax checkedExpression,
        out MemberAccessExpressionSyntax memberAccess,
        out AssignmentExpressionSyntax assignment)
    {
        checkedExpression = null!;
        memberAccess = null!;
        assignment = null!;

        if (!TryGetNullCheckedExpression(ifStatement.Condition, out checkedExpression))
            return false;

        if (!TryGetSingleStatement(ifStatement.Statement, out var singleStatement))
            return false;

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax a })
            return false;

        if (a.Kind() != SyntaxKind.SimpleAssignmentExpression)
            return false;

        if (a.Left is not MemberAccessExpressionSyntax ma)
            return false;

        if (!AreEquivalentIgnoringParentheses(checkedExpression, ma.Expression))
            return false;

        memberAccess = ma;
        assignment = a;
        return true;
    }

    public static bool TryGetNullCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax checkedExpression)
    {
        checkedExpression = null!;

        // Support: x != null
        if (condition is not BinaryExpressionSyntax binary ||
            binary.Kind() != SyntaxKind.NotEqualsExpression)
        {
            return false;
        }

        if (IsNullLiteral(binary.Right))
        {
            checkedExpression = binary.Left;
            return true;
        }

        if (!IsNullLiteral(binary.Left)) return false;
        checkedExpression = binary.Right;
        return true;

    }

    public static bool TryGetSingleStatement(StatementSyntax statement, out StatementSyntax singleStatement)
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

    public static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }

    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        return expression is LiteralExpressionSyntax literal && literal.Kind() == SyntaxKind.NullLiteralExpression;
    }

    private static bool AreEquivalentIgnoringParentheses(ExpressionSyntax left, ExpressionSyntax right)
    {
        left = StripParentheses(left);
        right = StripParentheses(right);

        return SyntaxFactory.AreEquivalent(left, right);
    }
}