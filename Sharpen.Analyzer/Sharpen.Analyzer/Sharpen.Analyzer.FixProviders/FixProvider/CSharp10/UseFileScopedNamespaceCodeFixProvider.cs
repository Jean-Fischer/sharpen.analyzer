using System;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFileScopedNamespaceCodeFixProvider))]
[Shared]
public sealed class UseFileScopedNamespaceCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseFileScopedNamespaceRule.Id);

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

        var ns = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
        if (ns == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use file-scoped namespace",
                createChangedDocument: c => ApplyFixAsync(context.Document, ns, c),
                equivalenceKey: "UseFileScopedNamespace"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp10OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && Common.CSharpLanguageVersion.IsCSharp10OrAbove(compilation);
    }

    private static async Task<Document> ApplyFixAsync(Document document, NamespaceDeclarationSyntax ns, CancellationToken ct)
    {
        // Convert:
        //   namespace X { /*members*/ }
        // to:
        //   namespace X;
        //   /*members*/
        //
        // In Roslyn, file-scoped namespaces are represented by FileScopedNamespaceDeclarationSyntax.
        // The members remain inside that node.

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // File-scoped namespaces cannot contain nested namespace declarations.
        // If the namespace contains nested namespaces, we only lift the outer namespace to file-scoped
        // and keep the inner namespaces as-is by moving them to the compilation unit.
        var hasNestedNamespace = ns.Members.OfType<NamespaceDeclarationSyntax>().Any();

        var members = hasNestedNamespace
            ? ns.Members.Where(m => m is not NamespaceDeclarationSyntax).ToList()
            : ns.Members.ToList();

        var fileScoped = SyntaxFactory.FileScopedNamespaceDeclaration(
                attributeLists: ns.AttributeLists,
                modifiers: ns.Modifiers,
                namespaceKeyword: ns.NamespaceKeyword,
                name: ns.Name,
                semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken),
                externs: default,
                usings: default,
                members: SyntaxFactory.List(members))
            .WithTriviaFrom(ns)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(ns, fileScoped);

        if (!hasNestedNamespace)
            return editor.GetChangedDocument();

        // Move nested namespaces to compilation unit level (after the file-scoped namespace).
        var root = (CompilationUnitSyntax)editor.GetChangedRoot();
        var inserted = root.Members.InsertRange(1, ns.Members.OfType<NamespaceDeclarationSyntax>());
        root = root.WithMembers(inserted).WithAdditionalAnnotations(Formatter.Annotation);
        return document.WithSyntaxRoot(root);
    }

}
