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

namespace Sharpen.Analyzer.FixProvider.CSharp3;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDefaultExpressionCodeFixProvider))]
[Shared]
public sealed class UseDefaultExpressionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
        Rules.GeneralRules.UseDefaultExpressionInReturnStatementsRule.Id,
        Rules.GeneralRules.UseDefaultExpressionInOptionalMethodParametersRule.Id,
        Rules.GeneralRules.UseDefaultExpressionInOptionalConstructorParametersRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Some analyzers report diagnostics on a token location (e.g. the 'return' keyword), while others
        // report directly on the DefaultExpressionSyntax. Be defensive and locate the default expression
        // within the containing statement/parameter.
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        var defaultExpression = node.FirstAncestorOrSelf<DefaultExpressionSyntax>()
                                ?? node.FirstAncestorOrSelf<ReturnStatementSyntax>()?.DescendantNodes()
                                    .OfType<DefaultExpressionSyntax>().FirstOrDefault()
                                ?? node.FirstAncestorOrSelf<ParameterSyntax>()?.DescendantNodes()
                                    .OfType<DefaultExpressionSyntax>().FirstOrDefault();

        if (defaultExpression is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use default literal",
                ct => UseDefaultLiteralAsync(context.Document, defaultExpression, ct),
                nameof(UseDefaultLiteralAsync)),
            diagnostic);
    }

    private static async Task<Document> UseDefaultLiteralAsync(Document document,
        DefaultExpressionSyntax defaultExpression, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var defaultLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)
            .WithTriviaFrom(defaultExpression);

        editor.ReplaceNode(defaultExpression, defaultLiteral);

        return editor.GetChangedDocument();
    }
}