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

namespace Sharpen.Analyzer.FixProvider.CSharp7;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseVarKeywordCodeFixProvider))]
[Shared]
public class UseVarKeywordCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseVarKeywordRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var diagnostic = context.Diagnostics.First();
        var typeNode = root.FindNode(diagnostic.Location.SourceSpan) as TypeSyntax;
        if (typeNode == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use 'var'",
                createChangedDocument: c => UseVarKeywordAsync(context.Document, typeNode, c),
                equivalenceKey: "UseVarKeyword"
            ),
            diagnostic
        );
    }

    private async Task<Document> UseVarKeywordAsync(Document document, TypeSyntax typeNode, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct);
        var varKeyword = SyntaxFactory.IdentifierName("var");
        var newRoot = root.ReplaceNode(typeNode, varKeyword);
        return document.WithSyntaxRoot(newRoot);
    }
}