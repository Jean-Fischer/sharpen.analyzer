namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Helper result for callers that need to distinguish:
///     - match failed (no candidate)
///     - match succeeded but unsafe (candidate blocked)
///     - match succeeded and safe
/// </summary>
public readonly struct FixProviderSafetyEvaluation
{
    public FixProviderSafetyOutcome Outcome { get; }

    public FixProviderSafetyResult? SafetyResult { get; }

    private FixProviderSafetyEvaluation(FixProviderSafetyOutcome outcome, FixProviderSafetyResult? safetyResult)
    {
        Outcome = outcome;
        SafetyResult = safetyResult;
    }

    public static FixProviderSafetyEvaluation MatchFailed()
    {
        return new FixProviderSafetyEvaluation(FixProviderSafetyOutcome.MatchFailed, null);
    }

    public static FixProviderSafetyEvaluation Safe()
    {
        return new FixProviderSafetyEvaluation(FixProviderSafetyOutcome.Safe, null);
    }

    public static FixProviderSafetyEvaluation Unsafe(FixProviderSafetyResult safetyResult)
    {
        return new FixProviderSafetyEvaluation(FixProviderSafetyOutcome.Unsafe, safetyResult);
    }
}