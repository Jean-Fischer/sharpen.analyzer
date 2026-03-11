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
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp9;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRecordTypeCodeFixProvider))]
[Shared]
public sealed class UseRecordTypeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseRecordTypeRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp9OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var classDecl = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDecl == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to record",
                createChangedDocument: c => ConvertToRecordAsync(context.Document, classDecl, c),
                equivalenceKey: "UseRecordType"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp9OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    private static async Task<Document> ConvertToRecordAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root == null)
            return document;

        // Keep the existing modifiers except `sealed`.
        var newModifiers = SyntaxFactory.TokenList(classDecl.Modifiers.Where(m => !m.IsKind(SyntaxKind.SealedKeyword)));

        // Convert `class` keyword to `record` keyword.
        var recordDecl = SyntaxFactory.RecordDeclaration(
                attributeLists: classDecl.AttributeLists,
                modifiers: newModifiers,
                keyword: SyntaxFactory.Token(classDecl.Keyword.LeadingTrivia, SyntaxKind.RecordKeyword, classDecl.Keyword.TrailingTrivia),
                identifier: classDecl.Identifier,
                typeParameterList: classDecl.TypeParameterList,
                parameterList: null,
                baseList: null,
                constraintClauses: classDecl.ConstraintClauses,
                openBraceToken: classDecl.OpenBraceToken,
                members: classDecl.Members,
                closeBraceToken: classDecl.CloseBraceToken,
                semicolonToken: classDecl.SemicolonToken);

        // Ensure we end with a semicolon for a simple record declaration.
        if (recordDecl.SemicolonToken.IsKind(SyntaxKind.None))
            recordDecl = recordDecl.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var newRoot = root.ReplaceNode(classDecl, recordDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}
