using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForGetOnlyIndexersCodeFixProvider)), Shared]
public sealed class UseExpressionBodyForGetOnlyIndexersCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForGetOnlyIndexersRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var indexer = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<IndexerDeclarationSyntax>().FirstOrDefault();
        if (indexer == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use expression-bodied indexer",
                createChangedDocument: ct => UseExpressionBodyAsync(context.Document, indexer, ct),
                equivalenceKey: "UseExpressionBodyForGetOnlyIndexers"),
            diagnostic);
    }

    private static async Task<Document> UseExpressionBodyAsync(Document document, IndexerDeclarationSyntax indexer, CancellationToken cancellationToken)
    {
        if (indexer.AccessorList == null || indexer.AccessorList.Accessors.Count != 1)
        {
            return document;
        }

        var getter = indexer.AccessorList.Accessors[0];
        if (!getter.IsKind(SyntaxKind.GetAccessorDeclaration))
        {
            return document;
        }

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(getter, out var expression))
        {
            return document;
        }

        var newIndexer = indexer
            .WithAccessorList(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression.WithoutTrivia()))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(indexer.GetTrailingTrivia())
            .WithLeadingTrivia(indexer.GetLeadingTrivia());

        newIndexer = newIndexer.WithExpressionBody(newIndexer.ExpressionBody!.WithLeadingTrivia(getter.GetLeadingTrivia()));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(indexer, newIndexer);
        return document.WithSyntaxRoot(newRoot);
    }
}
