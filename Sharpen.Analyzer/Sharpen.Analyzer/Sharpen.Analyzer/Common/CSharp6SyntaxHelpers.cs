using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Common;

public static class CSharp6SyntaxHelpers
{
    public static bool TryGetSingleReturnExpressionFromGetter(AccessorDeclarationSyntax getter, out ExpressionSyntax expression)
    {
        expression = null;

        if (getter == null)
        {
            return false;
        }

        if (getter.ExpressionBody != null)
        {
            return false;
        }

        if (getter.Body == null)
        {
            return false;
        }

        if (getter.Body.Statements.Count != 1)
        {
            return false;
        }

        if (getter.Body.Statements[0] is not ReturnStatementSyntax returnStatement)
        {
            return false;
        }

        if (returnStatement.Expression == null)
        {
            return false;
        }

        expression = returnStatement.Expression;
        return true;
    }

    public static bool TryGetStringLiteralValue(ExpressionSyntax expression, out string value)
    {
        value = null;

        if (expression is not LiteralExpressionSyntax literal)
        {
            return false;
        }

        if (!literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return false;
        }

        value = literal.Token.ValueText;
        return true;
    }

    public static bool IsNameofExpression(ExpressionSyntax expression)
    {
        if (expression is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        if (invocation.Expression is not IdentifierNameSyntax identifier)
        {
            return false;
        }

        return string.Equals(identifier.Identifier.ValueText, "nameof", StringComparison.Ordinal);
    }

    public static ITypeSymbol GetTypeSymbolOrNull(SemanticModel semanticModel, TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        if (semanticModel == null || typeSyntax == null)
        {
            return null;
        }

        return semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type;
    }
}
