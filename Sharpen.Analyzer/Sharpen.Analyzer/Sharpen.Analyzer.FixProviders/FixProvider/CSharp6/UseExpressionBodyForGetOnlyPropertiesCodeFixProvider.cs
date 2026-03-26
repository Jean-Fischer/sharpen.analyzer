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
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp6;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForGetOnlyPropertiesCodeFixProvider))]
[Shared]
public sealed class UseExpressionBodyForGetOnlyPropertiesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.UseExpressionBodyForGetOnlyPropertiesRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var property = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>().FirstOrDefault();
        if (property == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use expression-bodied property",
                ct => UseExpressionBodyAsync(context.Document, property, ct),
                "UseExpressionBodyForGetOnlyProperties"),
            diagnostic);
    }

    private static async Task<Document> UseExpressionBodyAsync(Document document, PropertyDeclarationSyntax property,
        CancellationToken cancellationToken)
    {
        if (property.AccessorList == null || property.AccessorList.Accessors.Count != 1) return document;

        var getter = property.AccessorList.Accessors[0];
        if (!getter.IsKind(SyntaxKind.GetAccessorDeclaration)) return document;

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(getter, out var expression)) return document;

        var generator = SyntaxGenerator.GetGenerator(document);

        var newProperty = property
            .WithAccessorList(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(expression.WithoutTrivia()))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            .WithTrailingTrivia(property.GetTrailingTrivia())
            .WithLeadingTrivia(property.GetLeadingTrivia());

        // Preserve trivia around the getter body as best-effort by attaching it to the arrow clause.
        newProperty =
            newProperty.WithExpressionBody(newProperty.ExpressionBody.WithLeadingTrivia(getter.GetLeadingTrivia()));

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var newRoot = root.ReplaceNode(property, newProperty);
        return document.WithSyntaxRoot(newRoot);
    }
}