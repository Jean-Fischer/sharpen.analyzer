using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Placeholder safety checker for a future null-check modernization fix provider.
/// </summary>
public sealed class NullCheckSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => FixProviderSafetyResult.Unsafe(
            reasonId: "null-check-not-implemented",
            message: "Null-check safety checker placeholder: corresponding fix provider not implemented yet.");
}
