namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Stages of the unified fix-provider safety pipeline.
/// </summary>
public enum FixProviderSafetyStage
{
    /// <summary>
    ///     No stage (used for safe results).
    /// </summary>
    None = 0,

    /// <summary>
    ///     Global, cross-cutting safety gate (formerly executed by the first-pass safety runner).
    /// </summary>
    Global = 1,

    /// <summary>
    ///     Local, fix-provider-specific safety checker (an <see cref="IFixProviderSafetyChecker" />).
    /// </summary>
    Local = 2
}