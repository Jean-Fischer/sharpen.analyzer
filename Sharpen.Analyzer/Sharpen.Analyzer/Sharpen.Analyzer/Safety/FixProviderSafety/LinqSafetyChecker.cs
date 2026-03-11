using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Placeholder safety checker for a future LINQ modernization fix provider.
/// </summary>
public sealed class LinqSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => FixProviderSafetyResult.Unsafe(
            FixProviderSafetyStage.Local,
            reasonId: "linq-not-implemented",
            message: "LINQ safety checker placeholder: corresponding fix provider not implemented yet.");
}
