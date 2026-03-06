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
using Microsoft.CodeAnalysis.Formatting;

namespace Sharpen.Analyzer.FixProvider.CSharp10;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGlobalUsingDirectiveCodeFixProvider))]
[Shared]
public sealed class UseGlobalUsingDirectiveCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseGlobalUsingDirectiveRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp10OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var usingDirective = node.FirstAncestorOrSelf<UsingDirectiveSyntax>();
        if (usingDirective == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use global using",
                createChangedDocument: c => ApplyFixAsync(context.Document, usingDirective, c),
                equivalenceKey: "UseGlobalUsing"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp10OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && Common.CSharpLanguageVersion.IsCSharp10OrAbove(compilation);
    }

    private static async Task<Document> ApplyFixAsync(Document document, UsingDirectiveSyntax usingDirective, CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Replace `using` with `global using` while preserving trivia.
        // Keep the original node trivia on the new `global` token.
        var globalToken = SyntaxFactory.Token(SyntaxKind.GlobalKeyword)
            .WithLeadingTrivia(usingDirective.GetLeadingTrivia())
            .WithTrailingTrivia(SyntaxFactory.Space);

        var updated = usingDirective
            .WithLeadingTrivia(SyntaxFactory.TriviaList())
            .WithGlobalKeyword(globalToken)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(usingDirective, updated);
        return editor.GetChangedDocument();
    }
}
