using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class ThreadSleepToTaskDelayCodeFixHelper
{
    internal static async Task RegisterAsync(
        CodeFixContext context,
        string title,
        string equivalenceKey)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var invocation = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
        if (invocation is null) return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(invocation)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                c => ApplyFixAsync(context.Document, invocation, semanticModel, c),
                equivalenceKey),
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

        // Thread.Sleep(arg) -> await Task.Delay(arg)
        // Reuse the original argument list to preserve trivia (e.g., comments) inside arguments.
        // (This is safe here because we replace nodes within the same syntax root.)
        var args = invocation.ArgumentList;

        var delayInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Task"),
                SyntaxFactory.IdentifierName("Delay")),
            args);

        // Preserve trivia from original invocation.
        delayInvocation = delayInvocation
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        ExpressionSyntax replacement = SyntaxFactory.AwaitExpression(delayInvocation)
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(invocation, replacement);

        // Ensure containing callable is async.
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, invocation, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}