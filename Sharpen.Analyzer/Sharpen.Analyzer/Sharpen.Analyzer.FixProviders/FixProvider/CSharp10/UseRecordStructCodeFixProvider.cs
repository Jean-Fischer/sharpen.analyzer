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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRecordStructCodeFixProvider))]
[Shared]
public sealed class UseRecordStructCodeFixProvider : CSharp10OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseRecordStructRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var decl = node.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (decl == null)
            return;

        RegisterCodeFix(
            context,
            diagnostic,
            title: "Use record struct",
            equivalenceKey: "UseRecordStruct",
            createChangedDocument: c => ApplyFixAsync(context.Document, decl, c));
    }

    private static async Task<Document> ApplyFixAsync(Document document, StructDeclarationSyntax decl, CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Prefer creating a RecordDeclarationSyntax (kind: record struct) so the resulting syntax node kind
        // is RecordStructDeclaration (tests expect that).
        var recordDecl = SyntaxFactory.RecordDeclaration(
                kind: SyntaxKind.RecordStructDeclaration,
                attributeLists: decl.AttributeLists,
                modifiers: decl.Modifiers,
                keyword: SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                classOrStructKeyword: SyntaxFactory.Token(SyntaxKind.StructKeyword),
                identifier: decl.Identifier,
                typeParameterList: decl.TypeParameterList,
                parameterList: null,
                baseList: decl.BaseList,
                constraintClauses: decl.ConstraintClauses,
                openBraceToken: decl.OpenBraceToken,
                members: decl.Members,
                closeBraceToken: decl.CloseBraceToken,
                semicolonToken: decl.SemicolonToken)
            .WithTriviaFrom(decl)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(decl, recordDecl);
        return editor.GetChangedDocument();
    }
}
