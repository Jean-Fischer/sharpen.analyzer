using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class ExpressionBodiedAccessorCodeFixHelper
{
    internal static Task<Document> UseExpressionBodyForGetAccessorAsync<TAncestor>(
        Document document,
        AccessorDeclarationSyntax accessor,
        CancellationToken cancellationToken,
        Func<AccessorListSyntax, bool>? additionalAccessorListPredicate = null)
        where TAncestor : SyntaxNode
    {
        return UseExpressionBodyAsync<TAncestor>(
            document,
            accessor,
            cancellationToken,
            SyntaxKind.GetAccessorDeclaration,
            static a =>
                CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(a, out var expression)
                    ? expression
                    : null,
            additionalAccessorListPredicate);
    }

    internal static Task<Document> UseExpressionBodyForSetAccessorAsync<TAncestor>(
        Document document,
        AccessorDeclarationSyntax accessor,
        CancellationToken cancellationToken)
        where TAncestor : SyntaxNode
    {
        return UseExpressionBodyAsync<TAncestor>(
            document,
            accessor,
            cancellationToken,
            SyntaxKind.SetAccessorDeclaration,
            static a =>
            {
                if (a.Body == null) return null;

                if (a.Body.Statements.Count != 1) return null;

                if (a.Body.Statements[0] is not ExpressionStatementSyntax expressionStatement) return null;

                return expressionStatement.Expression;
            },
            null);
    }

    private static async Task<Document> UseExpressionBodyAsync<TAncestor>(
        Document document,
        AccessorDeclarationSyntax accessor,
        CancellationToken cancellationToken,
        SyntaxKind accessorKind,
        Func<AccessorDeclarationSyntax, ExpressionSyntax?> getExpressionFromAccessor,
        Func<AccessorListSyntax, bool>? additionalAccessorListPredicate)
        where TAncestor : SyntaxNode
    {
        if (!accessor.IsKind(accessorKind)) return document;

        if (accessor.ExpressionBody != null) return document;

        if (accessor.FirstAncestorOrSelf<TAncestor>() == null) return document;

        if (accessor.Parent is not AccessorListSyntax accessorList) return document;

        if (additionalAccessorListPredicate != null && !additionalAccessorListPredicate(accessorList)) return document;

        var expression = getExpressionFromAccessor(accessor);
        if (expression == null) return document;

        var newAccessor = accessor
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression.WithoutTrivia()))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(accessor.GetTrailingTrivia())
            .WithLeadingTrivia(accessor.GetLeadingTrivia());

        // Preserve trivia around the accessor body as best-effort by attaching it to the arrow clause.
        newAccessor =
            newAccessor.WithExpressionBody(newAccessor.ExpressionBody!.WithLeadingTrivia(accessor.GetLeadingTrivia()));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var newRoot = root.ReplaceNode(accessor, newAccessor);
        return document.WithSyntaxRoot(newRoot);
    }
}