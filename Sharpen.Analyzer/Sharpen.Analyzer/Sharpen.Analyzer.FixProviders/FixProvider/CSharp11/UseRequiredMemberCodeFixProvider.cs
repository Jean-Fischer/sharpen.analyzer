using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp11;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRequiredMemberCodeFixProvider))]
[Shared]
public sealed class UseRequiredMemberCodeFixProvider : CSharp11OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp11Rules.UseRequiredMemberRule.Id);

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is not PropertyDeclarationSyntax property)
            return Task.CompletedTask;

        if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)))
            return Task.CompletedTask;

        RegisterCodeFix(
            context,
            diagnostic,
            "Add required modifier",
            nameof(UseRequiredMemberCodeFixProvider),
            ct => AddRequiredAsync(context.Document, property, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> AddRequiredAsync(Document document, PropertyDeclarationSyntax property,
        CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Insert 'required' after accessibility modifiers (public/protected/internal/private) and before other modifiers.
        var modifiers = property.Modifiers;
        var insertIndex = 0;
        for (var i = 0; i < modifiers.Count; i++)
        {
            var kind = modifiers[i].Kind();
            if (kind is SyntaxKind.PublicKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword
                or SyntaxKind.InternalKeyword)
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