namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Result of a fix-provider safety evaluation.
///     This is intentionally similar to <see cref="SafetyResult" />, but scoped to the fix-provider safety layer.
/// </summary>
public readonly struct FixProviderSafetyResult
{
    public bool IsSafe { get; }

    public string? ReasonId { get; }

    public string? Message { get; }

    /// <summary>
    ///     Indicates which stage of the unified safety pipeline produced this result.
    /// </summary>
    public FixProviderSafetyStage Stage { get; }

    public FixProviderSafetyResult(
        bool isSafe,
        FixProviderSafetyStage stage,
        string? reasonId = null,
        string? message = null)
    {
        IsSafe = isSafe;
        Stage = stage;
        ReasonId = reasonId;
        Message = message;
    }

    public static FixProviderSafetyResult Safe()
    {
        return new FixProviderSafetyResult(true, FixProviderSafetyStage.None);
    }

    public static FixProviderSafetyResult Unsafe(
        FixProviderSafetyStage stage,
        string reasonId,
        string? message = null)
    {
        return new FixProviderSafetyResult(false, stage, reasonId, message);
    }
}