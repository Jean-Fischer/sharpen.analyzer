using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Common;

public static class CSharp6SyntaxHelpers
{
    public static bool TryGetSingleReturnExpressionFromGetter(AccessorDeclarationSyntax? getter,
        out ExpressionSyntax? expression)
    {
        expression = null;

        if (getter == null) return false;

        if (getter.ExpressionBody != null) return false;

        if (getter.Body == null) return false;

        if (getter.Body.Statements.Count != 1) return false;

        if (getter.Body.Statements[0] is not ReturnStatementSyntax returnStatement) return false;

        if (returnStatement.Expression == null) return false;

        expression = returnStatement.Expression;
        return true;
    }

    public static bool TryGetStringLiteralValue(ExpressionSyntax expression, out string? value)
    {
        value = null;

        if (expression is not LiteralExpressionSyntax literal) return false;

        if (!literal.IsKind(SyntaxKind.StringLiteralExpression)) return false;

        value = literal.Token.ValueText;
        return true;
    }

    public static bool IsNameofExpression(ExpressionSyntax expression)
    {
        if (expression is not InvocationExpressionSyntax invocation) return false;

        return invocation.Expression is IdentifierNameSyntax identifier && string.Equals(identifier.Identifier.ValueText, "nameof", StringComparison.Ordinal);
    }
}