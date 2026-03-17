using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety;

public sealed class NoopFirstPassSafetyCheck : IFirstPassSafetyCheck
{
    public SafetyResult IsSafe(
        SyntaxTree? syntaxTree,
        SemanticModel semanticModel,
        Diagnostic? diagnostic,
        CancellationToken cancellationToken = default)
    {
        return SafetyResult.Safe();
    }
}