using System;

namespace Sharpen.Analyzer.Safety;

/// <summary>
/// Global entry point for the first-pass safety gate.
/// </summary>
/// <remarks>
/// This is a strict, opt-in gate that runs after a fix provider has identified a match candidate,
/// but before it registers a code fix. If any check reports <see cref="SafetyResult.IsSafe"/> = false,
/// the fix provider must not offer a transformation.
/// </remarks>
public static class FirstPassSafety
{
    /// <summary>
    /// The configured safety gate.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="FirstPassSafetyGate.Empty"/>.
    /// </remarks>
    public static FirstPassSafetyGate Gate { get; set; } = FirstPassSafetyGate.Empty;

    /// <summary>
    /// Optional hook invoked when a match is found but blocked by the safety gate.
    /// </summary>
    public static Action<SafetyResult>? UnsafeLogger { get; set; }
}
