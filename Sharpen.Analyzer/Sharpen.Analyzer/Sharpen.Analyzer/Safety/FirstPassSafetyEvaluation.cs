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

    public static FirstPassSafetyEvaluation MatchFailed()
    {
        return new FirstPassSafetyEvaluation(FirstPassSafetyOutcome.MatchFailed, null);
    }

    public static FirstPassSafetyEvaluation Safe()
    {
        return new FirstPassSafetyEvaluation(FirstPassSafetyOutcome.Safe, null);
    }

    public static FirstPassSafetyEvaluation Unsafe(SafetyResult safetyResult)
    {
        return new FirstPassSafetyEvaluation(FirstPassSafetyOutcome.Unsafe, safetyResult);
    }
}