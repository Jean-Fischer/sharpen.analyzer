using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Placeholder safety checker for a future null-check modernization fix provider.
/// </summary>
public sealed class NullCheckSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => FixProviderSafetyResult.Unsafe(
            FixProviderSafetyStage.Local,
            reasonId: "null-check-not-implemented",
            message: "Null-check safety checker placeholder: corresponding fix provider not implemented yet.");
}
