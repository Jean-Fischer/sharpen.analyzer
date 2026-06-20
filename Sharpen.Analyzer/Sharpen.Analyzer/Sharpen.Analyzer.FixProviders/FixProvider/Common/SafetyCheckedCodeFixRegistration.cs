using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class SafetyCheckedCodeFixRegistration
{
    public static async Task RegisterAsync<TNode>(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        Func<SyntaxNode, Diagnostic, TNode?> tryGetTargetNode,
        Func<CodeFixContext, SyntaxNode, Diagnostic, TNode, Task> registerCodeFixesAsync,
        IFixProviderSafetyChecker safetyChecker)
        where TNode : SyntaxNode
    {
        if (tryGetTargetNode is null)
            throw new ArgumentNullException(nameof(tryGetTargetNode));
        if (registerCodeFixesAsync is null)
            throw new ArgumentNullException(nameof(registerCodeFixesAsync));
        if (safetyChecker is null)
            throw new ArgumentNullException(nameof(safetyChecker));

        FixProviderSafetyMappingValidator.EnsureValidated();

        var targetNode = tryGetTargetNode(root, diagnostic);
        if (targetNode is null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            safetyChecker,
            root.SyntaxTree,
            semanticModel,
            diagnostic,
            true,
            context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        await registerCodeFixesAsync(context, root, diagnostic, targetNode).ConfigureAwait(false);
    }
}
