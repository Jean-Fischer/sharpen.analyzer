using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Placeholder safety checker for a future switch-expression modernization fix provider.
/// </summary>
public sealed class SwitchExpressionSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        return FixProviderSafetyResult.Unsafe(
            FixProviderSafetyStage.Local,
            "switch-expression-not-implemented",
            "Switch-expression safety checker placeholder: corresponding fix provider not implemented yet.");
    }
}