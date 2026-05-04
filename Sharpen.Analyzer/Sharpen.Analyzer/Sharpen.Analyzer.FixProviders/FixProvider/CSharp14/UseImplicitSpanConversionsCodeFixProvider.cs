using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseImplicitSpanConversionsCodeFixProvider))]
[Shared]
public sealed class UseImplicitSpanConversionsCodeFixProvider
    : CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<InvocationExpressionSyntax, ImplicitSpanConversionsSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseImplicitSpanConversionsRule.Id);

    protected override InvocationExpressionSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node as InvocationExpressionSyntax;
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        InvocationExpressionSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            CSharp14Rules.UseImplicitSpanConversionsRule.Title.ToString(),
            nameof(UseImplicitSpanConversionsCodeFixProvider),
            ct => ApplyAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyAsync(Document document, InvocationExpressionSyntax asSpanInvocation,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        if (!(root.FindNode(asSpanInvocation.Span, getInnermostNodeForTie: true) is InvocationExpressionSyntax currentInvocation))
            return document;

        if (currentInvocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return document;

        // Replace: expr.AsSpan() -> expr
        var replacement = memberAccess.Expression.WithTriviaFrom(currentInvocation);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(currentInvocation, replacement);
        return editor.GetChangedDocument();
    }
}
