using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Helper for analyzer/fix-provider paths to consistently distinguish:
/// - match failed (no candidate)
/// - match succeeded but unsafe (candidate blocked)
/// - match succeeded and safe
/// </summary>
public static class FixProviderSafetyRunner
{
    public static FixProviderSafetyEvaluation EvaluateOrMatchFailed(
        IFixProviderSafetyChecker checker,
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        bool matchSucceeded,
        CancellationToken cancellationToken = default)
    {
        if (checker is null)
            throw new ArgumentNullException(nameof(checker));

        if (!matchSucceeded)
            return FixProviderSafetyEvaluation.MatchFailed();

        var safety = checker.IsSafe(document, semanticModel, diagnostic, cancellationToken);
        if (!safety.IsSafe)
            return FixProviderSafetyEvaluation.Unsafe(safety);

        return FixProviderSafetyEvaluation.Safe();
    }

    public static FixProviderSafetyEvaluation Evaluate(
        SemanticModel semanticModel,
        Type fixProviderType,
        SyntaxNode node,
        Diagnostic? diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (semanticModel is null)
            throw new ArgumentNullException(nameof(semanticModel));
        if (fixProviderType is null)
            throw new ArgumentNullException(nameof(fixProviderType));
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        // Analyzer path: we don't have a Document or a Diagnostic instance.
        // Until checkers are expanded to use the matched node, treat this as "safe".
        // The fix provider path remains gated via EvaluateOrMatchFailed(...).
        return FixProviderSafetyEvaluation.Safe();
    }
}
