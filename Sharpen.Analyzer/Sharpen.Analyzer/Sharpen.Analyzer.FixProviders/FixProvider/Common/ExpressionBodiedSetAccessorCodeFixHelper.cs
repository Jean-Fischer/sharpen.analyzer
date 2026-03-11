using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class ExpressionBodiedSetAccessorCodeFixHelper
{
    internal static async Task<Document> UseExpressionBodyAsync<TAncestor>(
        Document document,
        AccessorDeclarationSyntax accessor,
        CancellationToken cancellationToken)
        where TAncestor : SyntaxNode
    {
        if (!accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
        {
            return document;
        }

        if (accessor.ExpressionBody != null)
        {
            return document;
        }

        if (accessor.FirstAncestorOrSelf<TAncestor>() == null)
        {
            return document;
        }

        if (accessor.Body == null)
        {
            return document;
        }

        if (accessor.Body.Statements.Count != 1)
        {
            return document;
        }

        if (accessor.Body.Statements[0] is not ExpressionStatementSyntax expressionStatement)
        {
            return document;
        }

        var expression = expressionStatement.Expression;

        var newAccessor = accessor
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression.WithoutTrivia()))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(accessor.GetTrailingTrivia())
            .WithLeadingTrivia(accessor.GetLeadingTrivia());

        // Preserve trivia around the setter body as best-effort by attaching it to the arrow clause.
        newAccessor = newAccessor.WithExpressionBody(newAccessor.ExpressionBody!.WithLeadingTrivia(accessor.GetLeadingTrivia()));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(accessor, newAccessor);
        return document.WithSyntaxRoot(newRoot);
    }
}
