using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
///     Safety checker for converting array creation expressions to collection expressions.
/// </summary>
public sealed class CollectionExpressionSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        // Conservative first pass:
        // - ensure C#
        // - ensure semantic model exists
        // - rely on existing FirstPassSafety for deeper checks until this checker is expanded

        var langCheck = SafetyCheckHelpers.UnsafeIfNotCSharp(syntaxTree, "not-csharp", "SyntaxTree is not C#.");
        if (!langCheck.IsSafe)
            return langCheck;

        if (semanticModel is null)
            return SafetyCheckHelpers.Unsafe("no-semantic-model", "SemanticModel is required.");

        if (diagnostic is null)
            return SafetyCheckHelpers.Unsafe("no-diagnostic", "Diagnostic is required.");

        return SafetyCheckHelpers.Safe();
    }
}