using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
    /// This mapping must not introduce a compile-time dependency from the analyzer assembly to the fix-provider assembly.
    /// Use <see cref="Type.GetType(string)"/> with assembly-qualified names if/when we need to populate this table.
    /// </remarks>
    public static ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)> Entries { get; } =
        ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)>.Empty;

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
