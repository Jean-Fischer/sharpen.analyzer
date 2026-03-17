using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;
using CSharpLanguageVersion = Sharpen.Analyzer.Common.CSharpLanguageVersion;

namespace Sharpen.Analyzer.FixProvider.CSharp9;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRecordTypeCodeFixProvider))]
[Shared]
public sealed class UseRecordTypeCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseRecordTypeRule.Id);

    protected override async Task<bool> ShouldRegisterFixesAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var classDecl = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDecl is null)
            return Task.CompletedTask;

        RegisterCodeFix(
            context,
            diagnostic,
            "Convert to record",
            "UseRecordType",
            c => ConvertToRecordAsync(context.Document, classDecl, c));

        return Task.CompletedTask;
    }

    private static async Task<Document> ConvertToRecordAsync(Document document, ClassDeclarationSyntax classDecl,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Keep the existing modifiers except `sealed`.
        var newModifiers = SyntaxFactory.TokenList(classDecl.Modifiers.Where(m => !m.IsKind(SyntaxKind.SealedKeyword)));

        // Convert `class` keyword to `record` keyword.
        var recordDecl = SyntaxFactory.RecordDeclaration(
            classDecl.AttributeLists,
            newModifiers,
            SyntaxFactory.Token(classDecl.Keyword.LeadingTrivia, SyntaxKind.RecordKeyword,
                classDecl.Keyword.TrailingTrivia),
            classDecl.Identifier,
            classDecl.TypeParameterList,
            null,
            null,
            classDecl.ConstraintClauses,
            classDecl.OpenBraceToken,
            classDecl.Members,
            classDecl.CloseBraceToken,
            classDecl.SemicolonToken);

        // Ensure we end with a semicolon for a simple record declaration.
        if (recordDecl.SemicolonToken.IsKind(SyntaxKind.None))
            recordDecl = recordDecl.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var newRoot = root.ReplaceNode(classDecl, recordDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}