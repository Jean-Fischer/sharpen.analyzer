using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Sharpen.Analyzer.FixProvider.CSharp10;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Canonical mapping between fix providers and their dedicated safety checkers.
/// </summary>
public static class FixProviderSafetyMapping
{
    /// <summary>
    /// Returns the canonical mapping table.
    /// </summary>
    /// <remarks>
    /// Keep this list sorted by fix provider type name for merge-friendliness.
    /// </remarks>
    public static ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)> Entries { get; } =
        ImmutableArray.Create(
            // NOTE: Initial set required by spec.
            (FixProviderType: typeof(UseCollectionExpressionCodeFixProvider), SafetyCheckerType: typeof(CollectionExpressionSafetyChecker)),
            (FixProviderType: typeof(UseInterpolatedStringCodeFixProvider), SafetyCheckerType: typeof(StringInterpolationSafetyChecker))

            // NOTE: Spec-required families that do not exist yet in this codebase.
            // Keep placeholders so the mapping table remains aligned with the spec.
            // (FixProviderType: typeof(NullCheckFixProvider), SafetyCheckerType: typeof(NullCheckSafetyChecker)),
            // (FixProviderType: typeof(SwitchExpressionFixProvider), SafetyCheckerType: typeof(SwitchExpressionSafetyChecker)),
            // (FixProviderType: typeof(LinqFixProvider), SafetyCheckerType: typeof(LinqSafetyChecker))
        );

    public static IReadOnlyDictionary<Type, Type> ToDictionary()
    {
        var dict = new Dictionary<Type, Type>();

        foreach (var (fixProviderType, safetyCheckerType) in Entries)
        {
            if (dict.ContainsKey(fixProviderType))
                throw new InvalidOperationException($"Duplicate fix provider mapping: {fixProviderType.FullName}");

            dict.Add(fixProviderType, safetyCheckerType);
        }

        return dict;
    }

    public static Type GetSafetyCheckerType(Type fixProviderType)
    {
        if (fixProviderType is null)
            throw new ArgumentNullException(nameof(fixProviderType));

        var dict = ToDictionary();
        if (!dict.TryGetValue(fixProviderType, out var checkerType))
            throw new InvalidOperationException($"No safety checker mapping found for fix provider: {fixProviderType.FullName}");

        return checkerType;
    }

    public static void ValidateUniqueness()
    {
        var duplicates = Entries
            .GroupBy(e => e.FixProviderType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            var msg = "Duplicate fix provider mapping entries: " + string.Join(", ", duplicates.Select(t => t.FullName));
            throw new InvalidOperationException(msg);
        }
    }
}
