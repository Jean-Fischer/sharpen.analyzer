using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Safety checker for a specific fix provider.
/// </summary>
/// <remarks>
///     This is intended to be a focused, conservative check that can be used to gate:
///     - diagnostic reporting (analyzers)
///     - code action registration (code fix providers)
/// </remarks>
public interface IFixProviderSafetyChecker
{
    /// <summary>
    ///     Returns whether the transformation associated with <paramref name="diagnostic" /> is safe.
    /// </summary>
    FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default);
}