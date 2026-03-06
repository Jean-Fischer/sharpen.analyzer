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

namespace Sharpen.Analyzer.FixProvider.CSharp11;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRequiredMemberCodeFixProvider)), Shared]
public sealed class UseRequiredMemberCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp11Rules.UseRequiredMemberRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken).ConfigureAwait(false);
        if (compilation == null || !CSharpLanguageVersion.IsCSharp11OrAbove(compilation))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is not PropertyDeclarationSyntax property)
            return;

        if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add required modifier",
                createChangedDocument: ct => AddRequiredAsync(context.Document, property, ct),
                equivalenceKey: nameof(UseRequiredMemberCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> AddRequiredAsync(Document document, PropertyDeclarationSyntax property, CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Insert 'required' after accessibility modifiers (public/protected/internal/private) and before other modifiers.
        var modifiers = property.Modifiers;
        var insertIndex = 0;
        for (var i = 0; i < modifiers.Count; i++)
        {
            var kind = modifiers[i].Kind();
            if (kind is SyntaxKind.PublicKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.InternalKeyword)
            {
                insertIndex = i + 1;
                continue;
            }

            break;
        }

        var requiredToken = SyntaxFactory.Token(SyntaxKind.RequiredKeyword)
            .WithTrailingTrivia(SyntaxFactory.Space);

        var newModifiers = modifiers.Insert(insertIndex, requiredToken);
        var newProperty = property.WithModifiers(newModifiers);

        editor.ReplaceNode(property, newProperty);
        return editor.GetChangedDocument();
    }
}
