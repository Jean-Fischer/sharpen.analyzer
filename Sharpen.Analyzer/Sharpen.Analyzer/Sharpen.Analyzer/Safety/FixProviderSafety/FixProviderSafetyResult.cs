namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Result of a fix-provider safety evaluation.
/// </summary>
public readonly struct FixProviderSafetyResult
{
    public bool IsSafe { get; }

    public string? ReasonId { get; }

    public string? Message { get; }

    public FixProviderSafetyResult(bool isSafe, string? reasonId = null, string? message = null)
    {
        IsSafe = isSafe;
        ReasonId = reasonId;
        Message = message;
    }

    public static FixProviderSafetyResult Safe() => new(true);

    public static FixProviderSafetyResult Unsafe(string reasonId, string? message = null) => new(false, reasonId, message);
}
