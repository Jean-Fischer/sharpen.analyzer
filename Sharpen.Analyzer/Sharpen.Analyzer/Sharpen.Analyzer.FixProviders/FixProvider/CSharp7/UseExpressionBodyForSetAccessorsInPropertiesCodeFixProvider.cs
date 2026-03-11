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
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp7;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForSetAccessorsInPropertiesCodeFixProvider)), Shared]
public sealed class UseExpressionBodyForSetAccessorsInPropertiesCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForSetAccessorsInPropertiesRule.Id);

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
                title: "Use expression-bodied set accessor",
                createChangedDocument: ct => UseExpressionBodyAsync(context.Document, accessor, ct),
                equivalenceKey: "UseExpressionBodyForSetAccessorsInProperties"),
            diagnostic);
    }

    private static async Task<Document> UseExpressionBodyAsync(Document document, AccessorDeclarationSyntax accessor, CancellationToken cancellationToken)
    {
        if (!accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
        {
            return document;
        }

        if (accessor.ExpressionBody != null)
        {
            return document;
        }

        if (accessor.FirstAncestorOrSelf<PropertyDeclarationSyntax>() == null)
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
