using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety;

/// <summary>
/// Deterministic, short-circuiting orchestrator for first-pass safety checks.
/// </summary>
public sealed class FirstPassSafetyGate
{
    private readonly IReadOnlyList<IFirstPassSafetyCheck> _checks;

    /// <summary>
    /// Creates a new gate with checks evaluated in the order provided.
    /// </summary>
    public FirstPassSafetyGate(IEnumerable<IFirstPassSafetyCheck> checks)
    {
        if (checks is null)
            throw new ArgumentNullException(nameof(checks));

        // Deterministic order: registration order.
        _checks = checks.ToList();
    }

    /// <summary>
    /// Evaluates all configured checks and returns the first unsafe result.
    /// </summary>
    /// <remarks>
    /// - Checks are evaluated in a deterministic order.
    /// - Evaluation short-circuits on the first unsafe result.
    /// - <paramref name="cancellationToken"/> is checked before each check.
    /// </remarks>
    public SafetyResult Evaluate(
        SyntaxTree? syntaxTree,
        SemanticModel semanticModel,
        Diagnostic? diagnostic,
        CancellationToken cancellationToken = default)
    {
        foreach (var check in _checks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Some call sites (e.g. analyzer path) do not have a SyntaxTree/Diagnostic instance.
            // Checks must tolerate nulls and treat them as "insufficient context".
            var result = check.IsSafe(syntaxTree, semanticModel, diagnostic, cancellationToken);
            if (!result.IsSafe)
                return result;
        }

        return SafetyResult.Safe();
    }

    /// <summary>
    /// An empty gate that always returns <see cref="SafetyResult.Safe"/>.
    /// </summary>
    public static FirstPassSafetyGate Empty { get; } = new(Array.Empty<IFirstPassSafetyCheck>());
}
