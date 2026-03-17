using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Helpers.CSharp14;

public static class FieldBackedPropertyHelper
{
    public static bool TryGetBackingFieldFromGetter(
        SemanticModel semanticModel,
        AccessorDeclarationSyntax getAccessor,
        CancellationToken ct,
        out IFieldSymbol? backingFieldSymbol)
    {
        backingFieldSymbol = null;

        ExpressionSyntax? returnedExpression = null;

        if (getAccessor.ExpressionBody is not null)
        {
            returnedExpression = getAccessor.ExpressionBody.Expression;
        }
        else if (getAccessor.Body is not null)
        {
            var statements = getAccessor.Body.Statements;
            if (statements.Count != 1)
                return false;

            if (statements[0] is not ReturnStatementSyntax returnStatement)
                return false;

            returnedExpression = returnStatement.Expression;
        }

        if (returnedExpression is null)
            return false;

        var symbol = semanticModel.GetSymbolInfo(returnedExpression, ct).Symbol;
        if (symbol is not IFieldSymbol fieldSymbol)
            return false;

        backingFieldSymbol = fieldSymbol;
        return true;
    }
}