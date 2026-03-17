using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Placeholder safety checker for a future LINQ modernization fix provider.
/// </summary>
public sealed class LinqSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        return FixProviderSafetyResult.Unsafe(
            FixProviderSafetyStage.Local,
            "linq-not-implemented",
            "LINQ safety checker placeholder: corresponding fix provider not implemented yet.");
    }
}