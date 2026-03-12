using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp10;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseGlobalUsingDirectiveCodeFixProvider))]
[Shared]
public sealed class UseGlobalUsingDirectiveCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseGlobalUsingDirectiveRule.Id);

    protected override async Task<bool> ShouldRegisterFixesAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && Common.CSharpLanguageVersion.IsCSharp10OrAbove(compilation);
    }

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var usingDirective = node.FirstAncestorOrSelf<UsingDirectiveSyntax>();
        if (usingDirective is null)
            return Task.CompletedTask;

        RegisterCodeFix(
            context,
            diagnostic,
            title: "Use global using",
            equivalenceKey: "UseGlobalUsing",
            createChangedDocument: c => ApplyFixAsync(context.Document, usingDirective, c));

        return Task.CompletedTask;
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
