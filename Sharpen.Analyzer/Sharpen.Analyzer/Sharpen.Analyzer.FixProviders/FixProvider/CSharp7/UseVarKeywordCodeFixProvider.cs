using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp7;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseVarKeywordCodeFixProvider))]
[Shared]
public sealed class UseVarKeywordCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseVarKeywordRule.Id);

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var typeNode = root.FindNode(diagnostic.Location.SourceSpan) as TypeSyntax;
        if (typeNode is null)
            return Task.CompletedTask;

        RegisterCodeFix(
            context,
            diagnostic,
            "Use 'var'",
            "UseVarKeyword",
            c => UseVarKeywordAsync(context.Document, typeNode, c));

        return Task.CompletedTask;
    }

    private static async Task<Document> UseVarKeywordAsync(Document document, TypeSyntax typeNode, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var varKeyword = SyntaxFactory.IdentifierName("var");
        var newRoot = root.ReplaceNode(typeNode, varKeyword);
        return document.WithSyntaxRoot(newRoot);
    }
}