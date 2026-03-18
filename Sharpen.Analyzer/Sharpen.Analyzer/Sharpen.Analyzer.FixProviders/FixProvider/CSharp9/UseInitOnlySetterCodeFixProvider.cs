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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseInitOnlySetterCodeFixProvider))]
[Shared]
public sealed class UseInitOnlySetterCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.UseInitOnlySetterRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp9OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var property = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
        if (property?.AccessorList == null)
            return;

        var setAccessor =
            property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (setAccessor == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use 'init'",
                c => UseInitAsync(context.Document, setAccessor, c),
                "UseInitOnlySetter"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp9OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    private static async Task<Document> UseInitAsync(Document document, AccessorDeclarationSyntax setAccessor,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root == null)
            return document;

        // Drop the original `private` modifier: `init` already implies init-only semantics.
        // Keeping it would produce `private init;` which is not the intended modernization.
        var newModifiers =
            SyntaxFactory.TokenList(setAccessor.Modifiers.Where(m => !m.IsKind(SyntaxKind.PrivateKeyword)));

        var initAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
            .WithKeyword(SyntaxFactory.Token(setAccessor.Keyword.LeadingTrivia, SyntaxKind.InitKeyword,
                setAccessor.Keyword.TrailingTrivia))
            .WithAttributeLists(setAccessor.AttributeLists)
            .WithModifiers(newModifiers)
            .WithSemicolonToken(setAccessor.SemicolonToken)
            .WithLeadingTrivia(setAccessor.GetLeadingTrivia())
            .WithTrailingTrivia(setAccessor.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(setAccessor, initAccessor);
        return document.WithSyntaxRoot(newRoot);
    }
}