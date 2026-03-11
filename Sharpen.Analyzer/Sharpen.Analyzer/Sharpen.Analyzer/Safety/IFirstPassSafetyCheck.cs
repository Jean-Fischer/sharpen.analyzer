using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety;

/// <summary>
/// A single, conservative safety check that can block a code fix from being offered.
/// </summary>
public interface IFirstPassSafetyCheck
{
    /// <summary>
    /// Returns whether the transformation associated with <paramref name="diagnostic"/> is safe.
    /// </summary>
    /// <remarks>
    /// Some call sites (e.g. analyzer path) do not have a <see cref="Document"/> or a
    /// <see cref="Diagnostic"/> instance. Implementations must tolerate nulls and treat them as
    /// "insufficient context".
    /// </remarks>
    SafetyResult IsSafe(
        Document? document,
        SemanticModel semanticModel,
        Diagnostic? diagnostic,
        CancellationToken cancellationToken = default);
}
