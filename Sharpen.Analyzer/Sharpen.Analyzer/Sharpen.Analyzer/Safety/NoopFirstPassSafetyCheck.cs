using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety;

public sealed class NoopFirstPassSafetyCheck : IFirstPassSafetyCheck
{
    public SafetyResult IsSafe(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => SafetyResult.Safe();
}
