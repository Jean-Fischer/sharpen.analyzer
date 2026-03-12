using System;
using System.Composition;
using System.Linq;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFileScopedNamespaceCodeFixProvider))]
public sealed class UseFileScopedNamespaceCodeFixProvider : CSharp10OrAboveCodeFixProviderBase
{
    protected override string DiagnosticId => Rules.CSharp10Rules.UseFileScopedNamespaceRule.Id;

    protected override string Title => "Use file-scoped namespace";

    protected override string EquivalenceKey => "UseFileScopedNamespace";

    protected override Task<Document> CreateChangedDocumentAsync(Document document, SyntaxNode root, Diagnostic diagnostic, CancellationToken ct)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        var ns = node.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
        if (ns == null)
        {
            return Task.FromResult(document);
        }

        return ApplyFixAsync(document, ns, ct);
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
