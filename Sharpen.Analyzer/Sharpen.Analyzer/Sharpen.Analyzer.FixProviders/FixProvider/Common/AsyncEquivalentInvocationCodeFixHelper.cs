using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Engine.SharpenSuggestions.Common.AsyncAwaitAndAsyncStreams;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class AsyncEquivalentInvocationCodeFixHelper
{
    internal static async Task<Document> ApplyAsyncEquivalentAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null) return document;

        var asyncEquivalent = EquivalentAsynchronousMethodResolver.ResolveAsyncEquivalent(invocation, semanticModel);
        if (asyncEquivalent is null) return document;

        var rewrittenInvocation = RewriteInvocation(invocation, asyncEquivalent.Name);
        if (rewrittenInvocation is null) return document;

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // Await insertion rules (conservative):
        // - avoid double-await
        // - only apply in known-safe contexts
        var replacementExpression = ApplyAwaitIfNeeded(invocation, rewrittenInvocation, semanticModel);

        var newRoot = root.ReplaceNode(invocation, replacementExpression);
        return document.WithSyntaxRoot(newRoot);
    }

    private static InvocationExpressionSyntax? RewriteInvocation(InvocationExpressionSyntax invocation, string asyncMethodName)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return invocation.WithExpression(
                memberAccess.WithName(SyntaxFactory.IdentifierName(asyncMethodName)));
        }

        if (invocation.Expression is IdentifierNameSyntax)
        {
            return invocation.WithExpression(SyntaxFactory.IdentifierName(asyncMethodName));
        }

        // Unknown invocation shape (e.g., conditional access). Skip.
        return null;
    }

    private static ExpressionSyntax ApplyAwaitIfNeeded(
        InvocationExpressionSyntax originalInvocation,
        InvocationExpressionSyntax rewrittenInvocation,
        SemanticModel semanticModel)
    {
        // Preserve trivia on the invocation itself.
        rewrittenInvocation = rewrittenInvocation
            .WithLeadingTrivia(originalInvocation.GetLeadingTrivia())
            .WithTrailingTrivia(originalInvocation.GetTrailingTrivia());

        // Avoid double-await.
        if (originalInvocation.Parent is AwaitExpressionSyntax)
        {
            return rewrittenInvocation;
        }

        // Only add await when inside an async method/local function.
        if (!IsWithinAsyncCallable(originalInvocation, semanticModel))
        {
            return rewrittenInvocation;
        }

        // Expression statement: `X();` -> `await XAsync();`
        if (originalInvocation.Parent is ExpressionStatementSyntax)
        {
            return SyntaxFactory.AwaitExpression(rewrittenInvocation)
                .WithLeadingTrivia(originalInvocation.GetLeadingTrivia())
                .WithTrailingTrivia(originalInvocation.GetTrailingTrivia());
        }

        // Assignment RHS: `x = X();` -> `x = await XAsync();`
        if (originalInvocation.Parent is AssignmentExpressionSyntax assignment &&
            assignment.Right == originalInvocation)
        {
            return SyntaxFactory.AwaitExpression(rewrittenInvocation)
                .WithLeadingTrivia(originalInvocation.GetLeadingTrivia())
                .WithTrailingTrivia(originalInvocation.GetTrailingTrivia());
        }

        // Variable initializer: `var x = X();` -> `var x = await XAsync();`
        if (originalInvocation.Parent is EqualsValueClauseSyntax equalsValue &&
            equalsValue.Value == originalInvocation)
        {
            return SyntaxFactory.AwaitExpression(rewrittenInvocation)
                .WithLeadingTrivia(originalInvocation.GetLeadingTrivia())
                .WithTrailingTrivia(originalInvocation.GetTrailingTrivia());
        }

        // Return statement: `return X();` -> `return await XAsync();`
        if (originalInvocation.Parent is ReturnStatementSyntax)
        {
            return SyntaxFactory.AwaitExpression(rewrittenInvocation)
                .WithLeadingTrivia(originalInvocation.GetLeadingTrivia())
                .WithTrailingTrivia(originalInvocation.GetTrailingTrivia());
        }

        // Guardrails: unknown context, don't add await.
        return rewrittenInvocation;
    }

    private static bool IsWithinAsyncCallable(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var localFunction = invocation.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (localFunction != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(localFunction) as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        return false;
    }
}
