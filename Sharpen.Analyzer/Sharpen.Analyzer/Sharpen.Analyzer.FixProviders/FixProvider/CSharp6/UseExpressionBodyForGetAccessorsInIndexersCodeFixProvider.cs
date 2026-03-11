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

namespace Sharpen.Analyzer.FixProvider.CSharp6;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider)), Shared]
public sealed class UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForGetAccessorsInIndexersRule.Id);

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

        var accessor = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<AccessorDeclarationSyntax>().FirstOrDefault();
        if (accessor == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use expression-bodied get accessor",
                createChangedDocument: ct => UseExpressionBodyAsync(context.Document, accessor, ct),
                equivalenceKey: "UseExpressionBodyForGetAccessorsInIndexers"),
            diagnostic);
    }

    private static async Task<Document> UseExpressionBodyAsync(Document document, AccessorDeclarationSyntax accessor, CancellationToken cancellationToken)
    {
        if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
        {
            return document;
        }

        if (accessor.ExpressionBody != null)
        {
            return document;
        }

        if (accessor.Parent is not AccessorListSyntax accessorList)
        {
            return document;
        }

        // Must have a set accessor as well (otherwise C# 6 get-only indexer rule applies).
        if (accessorList.Accessors.Count <= 1)
        {
            return document;
        }

        if (accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>() == null)
        {
            return document;
        }

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(accessor, out var expression))
        {
            return document;
        }

        var newAccessor = accessor
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression.WithoutTrivia()))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(accessor.GetTrailingTrivia())
            .WithLeadingTrivia(accessor.GetLeadingTrivia());

        // Preserve trivia around the getter body as best-effort by attaching it to the arrow clause.
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
