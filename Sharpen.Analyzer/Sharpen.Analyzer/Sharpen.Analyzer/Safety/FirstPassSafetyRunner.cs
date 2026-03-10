using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety;

/// <summary>
/// Helper for fix providers to consistently distinguish:
/// - match failed (no candidate)
/// - match succeeded but unsafe (candidate blocked)
/// - match succeeded and safe
/// </summary>
public static class FirstPassSafetyRunner
{
    public static FirstPassSafetyEvaluation EvaluateOrMatchFailed(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        bool matchSucceeded,
        CancellationToken cancellationToken = default)
    {
        if (!matchSucceeded)
            return FirstPassSafetyEvaluation.MatchFailed();

        var safety = FirstPassSafety.Gate.Evaluate(document, semanticModel, diagnostic, cancellationToken);
        if (!safety.IsSafe)
        {
            FirstPassSafety.UnsafeLogger?.Invoke(safety);
            return FirstPassSafetyEvaluation.Unsafe(safety);
        }

        return FirstPassSafetyEvaluation.Safe();
    }
}
