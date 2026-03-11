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

namespace Sharpen.Analyzer.FixProvider.CSharp5;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitTaskInsteadOfCallingTaskWaitCodeFixProvider))]
[Shared]
public sealed class AwaitTaskInsteadOfCallingTaskWaitCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        if (node is not InvocationExpressionSyntax invocation) return;

        // Only support parameterless Wait() for code fix.
        if (invocation.ArgumentList.Arguments.Count != 0) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(invocation)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use await",
                createChangedDocument: c => ApplyFixAsync(context.Document, invocation, semanticModel, c),
                equivalenceKey: "UseAwait"),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // task.Wait() -> await task
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        var awaited = SyntaxFactory.AwaitExpression(memberAccess.Expression.WithoutTrivia())
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(invocation, awaited);
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, invocation, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}
