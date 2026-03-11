namespace Sharpen.Analyzer.Safety;

public readonly struct FirstPassSafetyEvaluation
{
    public FirstPassSafetyOutcome Outcome { get; }

    public SafetyResult? SafetyResult { get; }

    private FirstPassSafetyEvaluation(FirstPassSafetyOutcome outcome, SafetyResult? safetyResult)
    {
        Outcome = outcome;
        SafetyResult = safetyResult;
    }

    public static FirstPassSafetyEvaluation MatchFailed() => new(FirstPassSafetyOutcome.MatchFailed, safetyResult: null);

    public static FirstPassSafetyEvaluation Safe() => new(FirstPassSafetyOutcome.Safe, safetyResult: null);

    public static FirstPassSafetyEvaluation Unsafe(SafetyResult safetyResult) => new(FirstPassSafetyOutcome.Unsafe, safetyResult);
}
