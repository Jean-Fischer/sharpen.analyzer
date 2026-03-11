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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRecordStructCodeFixProvider))]
[Shared]
public sealed class UseRecordStructCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseRecordStructRule.Id);

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
        var decl = node.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (decl == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use record struct",
                createChangedDocument: c => ApplyFixAsync(context.Document, decl, c),
                equivalenceKey: "UseRecordStruct"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp10OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && Common.CSharpLanguageVersion.IsCSharp10OrAbove(compilation);
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
