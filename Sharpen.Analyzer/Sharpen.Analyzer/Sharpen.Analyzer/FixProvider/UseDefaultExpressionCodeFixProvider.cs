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

namespace Sharpen.Analyzer.FixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDefaultExpressionCodeFixProvider))]
[Shared]
public sealed class UseDefaultExpressionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        Rules.Rules.UseDefaultExpressionInReturnStatementsRule.Id,
        Rules.Rules.UseDefaultExpressionInOptionalMethodParametersRule.Id,
        Rules.Rules.UseDefaultExpressionInOptionalConstructorParametersRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        var defaultExpression = node.FirstAncestorOrSelf<DefaultExpressionSyntax>()
                              ?? node.DescendantNodesAndSelf().OfType<DefaultExpressionSyntax>().FirstOrDefault();

        if (defaultExpression is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use default literal",
                createChangedDocument: ct => UseDefaultLiteralAsync(context.Document, defaultExpression, ct),
                equivalenceKey: "Use default literal"),
            diagnostic);
    }

    private static async Task<Document> UseDefaultLiteralAsync(Document document, DefaultExpressionSyntax defaultExpression, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var defaultLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)
            .WithTriviaFrom(defaultExpression);

        editor.ReplaceNode(defaultExpression, defaultLiteral);

        return editor.GetChangedDocument();
    }
}
