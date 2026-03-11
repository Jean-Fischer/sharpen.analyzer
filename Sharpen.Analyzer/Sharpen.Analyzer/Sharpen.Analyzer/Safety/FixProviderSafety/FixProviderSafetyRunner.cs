using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Helper for analyzer/fix-provider paths to consistently distinguish:
/// - match failed (no candidate)
/// - match succeeded but unsafe (candidate blocked)
/// - match succeeded and safe
///
/// Unified pipeline stages:
/// 1) Global <see cref="FirstPassSafety"/> gate
/// 2) Local per-fix-provider <see cref="IFixProviderSafetyChecker"/>
///
/// Short-circuit rules:
/// - If match failed: return <see cref="FixProviderSafetyOutcome.MatchFailed"/>.
/// - If global gate is unsafe: return unsafe and do not evaluate local checker.
/// - If local checker is unsafe: return unsafe.
/// - Otherwise: safe.
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
        if (document is null)
            throw new ArgumentNullException(nameof(document));
        if (semanticModel is null)
            throw new ArgumentNullException(nameof(semanticModel));
        if (diagnostic is null)
            throw new ArgumentNullException(nameof(diagnostic));

        if (!matchSucceeded)
            return FixProviderSafetyEvaluation.MatchFailed();

        // 1) Global stage
        var globalSafety = FirstPassSafety.Gate.Evaluate(document, semanticModel, diagnostic, cancellationToken);
        if (!globalSafety.IsSafe)
        {
            FirstPassSafety.UnsafeLogger?.Invoke(globalSafety);
            return FixProviderSafetyEvaluation.Unsafe(
                FixProviderSafetyResult.Unsafe(
                    FixProviderSafetyStage.Global,
                    reasonId: globalSafety.ReasonId ?? "first-pass-unsafe",
                    message: globalSafety.Message));
        }

        // 2) Local stage
        var localSafety = checker.IsSafe(document, semanticModel, diagnostic, cancellationToken);
        if (!localSafety.IsSafe)
        {
            return FixProviderSafetyEvaluation.Unsafe(
                FixProviderSafetyResult.Unsafe(
                    FixProviderSafetyStage.Local,
                    reasonId: localSafety.ReasonId ?? "fix-provider-unsafe",
                    message: localSafety.Message));
        }

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

        // Analyzer path: we don't have a Document instance, but we still want to apply the same
        // global policy gate (generated code, feature flags, etc.) so diagnostics and code actions
        // are aligned.
        var globalSafety = FirstPassSafety.Gate.Evaluate(
            document: null,
            semanticModel: semanticModel,
            diagnostic: diagnostic,
            cancellationToken: cancellationToken);

        if (!globalSafety.IsSafe)
        {
            FirstPassSafety.UnsafeLogger?.Invoke(globalSafety);
            return FixProviderSafetyEvaluation.Unsafe(
                FixProviderSafetyResult.Unsafe(
                    FixProviderSafetyStage.Global,
                    reasonId: globalSafety.ReasonId ?? "first-pass-unsafe",
                    message: globalSafety.Message));
        }

        // Local stage: resolve the mapped checker and evaluate it.
        // NOTE: Analyzer-side checkers currently require a Document instance. Until we expand the
        // checker contract, we conservatively treat analyzer-side local evaluation as "safe".
        // This still ensures the global gate is consistently applied.
        //
        // TODO (pipeline-unification): extend IFixProviderSafetyChecker to support analyzer context.
        _ = fixProviderType;
        _ = node;

        return FixProviderSafetyEvaluation.Safe();
    }
}
