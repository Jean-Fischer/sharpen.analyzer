using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Extensions;

internal static class ExpressionSyntaxExtensions
{
    public static ExpressionSyntax WalkDownParentheses(this ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
