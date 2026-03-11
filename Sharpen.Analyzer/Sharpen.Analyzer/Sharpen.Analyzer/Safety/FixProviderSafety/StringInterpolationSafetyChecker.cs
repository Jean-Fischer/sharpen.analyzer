using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Safety checker for converting string.Format / string concatenation to interpolated strings.
/// </summary>
public sealed class StringInterpolationSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        Document document,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        // Conservative first pass:
        // - ensure C#
        // - ensure semantic model exists
        // - deeper checks (formatting behavior, side effects) can be added later

        var langCheck = SafetyCheckHelpers.UnsafeIfNotCSharp(document, reasonId: "not-csharp", message: "Document is not C#.");
        if (!langCheck.IsSafe)
            return langCheck;

        if (semanticModel is null)
            return SafetyCheckHelpers.Unsafe("no-semantic-model", "SemanticModel is required.");

        if (diagnostic is null)
            return SafetyCheckHelpers.Unsafe("no-diagnostic", "Diagnostic is required.");

        return SafetyCheckHelpers.Safe();
    }
}
