namespace Sharpen.Analyzer.Safety;

public readonly struct SafetyResult
{
    public bool IsSafe { get; }

    public string? ReasonId { get; }

    public string? Message { get; }

    public SafetyResult(bool isSafe, string? reasonId = null, string? message = null)
    {
        IsSafe = isSafe;
        ReasonId = reasonId;
        Message = message;
    }

    public static SafetyResult Safe()
    {
        return new SafetyResult(true);
    }

    public static SafetyResult Unsafe(string reasonId, string? message = null)
    {
        return new SafetyResult(false, reasonId, message);
    }
}