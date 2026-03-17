using System;
using System.Collections.Immutable;
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

internal static class TaskWaitToWhenCodeFixHelper
{
    internal static async Task RegisterAsync(
        CodeFixContext context,
        string title,
        string equivalenceKey,
        string? whenMethodName,
        Func<ImmutableArray<string>> fixableDiagnosticIds)
    {
        // NOTE: fixableDiagnosticIds is passed only to keep call sites minimal and consistent.
        // It is not used here, but it helps ensure providers remain thin wrappers.
        _ = fixableDiagnosticIds;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        if (node is not InvocationExpressionSyntax invocation) return;

        // Only offer fix when the invocation is used as a statement (return value not used).
        if (invocation.Parent is not ExpressionStatementSyntax) return;

        // For Task.WaitAll/WaitAny we support any args; for instance Wait() we only support parameterless.
        if (whenMethodName is null && invocation.ArgumentList.Arguments.Count != 0) return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(invocation)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                c => ApplyFixAsync(context.Document, invocation, semanticModel, whenMethodName, c),
                equivalenceKey),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        string? whenMethodName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        ExpressionSyntax awaitedExpression;

        if (whenMethodName is null)
        {
            // task.Wait() -> await task
            awaitedExpression = memberAccess.Expression.WithoutTrivia();
        }
        else
        {
            // Task.WaitAll(args) -> await Task.WhenAll(args)
            // Task.WaitAny(args) -> await Task.WhenAny(args)
            var whenMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(whenMethodName));
            awaitedExpression = invocation.WithExpression(whenMemberAccess).WithoutTrivia();
        }

        var awaited = SyntaxFactory.AwaitExpression(awaitedExpression)
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(invocation, awaited);
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, invocation, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}