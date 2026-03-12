using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class ExpressionBodiedSetAccessorCodeFixHelper
{
    internal static Task<Document> UseExpressionBodyAsync<TAncestor>(
        Document document,
        AccessorDeclarationSyntax accessor,
        CancellationToken cancellationToken)
        where TAncestor : SyntaxNode
        => ExpressionBodiedAccessorCodeFixHelper.UseExpressionBodyForSetAccessorAsync<TAncestor>(document, accessor, cancellationToken);
}
