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
        string whenMethodName,
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

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(invocation)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: c => ApplyFixAsync(context.Document, invocation, semanticModel, whenMethodName, c),
                equivalenceKey: equivalenceKey),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        string whenMethodName,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // Task.WaitAll(args) -> await Task.WhenAll(args)
        // Task.WaitAny(args) -> await Task.WhenAny(args)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        var whenMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName(whenMethodName));
        var whenInvocation = invocation.WithExpression(whenMemberAccess);

        var awaited = SyntaxFactory.AwaitExpression(whenInvocation.WithoutTrivia())
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(invocation, awaited);
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, invocation, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}
