using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Placeholder safety checker used when a fix provider does not exist yet in the codebase,
/// but the mapping table/spec requires a named checker type.
/// </summary>
/// <remarks>
/// This checker is intentionally conservative and returns unsafe.
/// Replace with a real checker when the corresponding fix provider is implemented.
/// </remarks>
public sealed class PlaceholderSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => FixProviderSafetyResult.Unsafe(
            FixProviderSafetyStage.Local,
            reasonId: "placeholder-checker",
            message: "Placeholder safety checker: real fix provider/checker not implemented yet.");
}
