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
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp10;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRecordStructCodeFixProvider))]
[Shared]
public sealed class UseRecordStructCodeFixProvider : CSharp10OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp10Rules.UseRecordStructRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var decl = node.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (decl == null)
            return;

        RegisterCodeFix(
            context,
            diagnostic,
            "Use record struct",
            "UseRecordStruct",
            c => ApplyFixAsync(context.Document, decl, c));
    }

    private static async Task<Document> ApplyFixAsync(Document document, StructDeclarationSyntax decl,
        CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Prefer creating a RecordDeclarationSyntax (kind: record struct) so the resulting syntax node kind
        // is RecordStructDeclaration (tests expect that).
        var recordDecl = SyntaxFactory.RecordDeclaration(
                SyntaxKind.RecordStructDeclaration,
                decl.AttributeLists,
                decl.Modifiers,
                SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                SyntaxFactory.Token(SyntaxKind.StructKeyword),
                decl.Identifier,
                decl.TypeParameterList,
                null,
                decl.BaseList,
                decl.ConstraintClauses,
                decl.OpenBraceToken,
                decl.Members,
                decl.CloseBraceToken,
                decl.SemicolonToken)
            .WithTriviaFrom(decl)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(decl, recordDecl);
        return editor.GetChangedDocument();
    }
}