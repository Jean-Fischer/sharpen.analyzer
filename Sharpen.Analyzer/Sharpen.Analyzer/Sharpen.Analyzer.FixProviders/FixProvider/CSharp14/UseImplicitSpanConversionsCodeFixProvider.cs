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
public sealed class UseImplicitSpanConversionsCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseImplicitSpanConversionsRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is not InvocationExpressionSyntax asSpanInvocation)
            return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new ImplicitSpanConversionsSafetyChecker(),
            root.SyntaxTree,
            semanticModel,
            diagnostic,
            true,
            context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        RegisterCodeFix(
            context,
            diagnostic,
            CSharp14Rules.UseImplicitSpanConversionsRule.Title.ToString(),
            nameof(UseImplicitSpanConversionsCodeFixProvider),
            ct => ApplyAsync(context.Document, asSpanInvocation, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, InvocationExpressionSyntax asSpanInvocation,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var currentInvocation =
            root.FindNode(asSpanInvocation.Span, getInnermostNodeForTie: true) as InvocationExpressionSyntax;
        if (currentInvocation is null)
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