using System;
using System.Collections.Generic;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public static class FixProviderSafetyMappingValidation
{
    /// <summary>
    /// Validates mapping completeness for the spec-required initial set.
    /// </summary>
    /// <remarks>
    /// This is intentionally scoped to the initial set described in the spec.
    /// It does not attempt to cover all fix providers in the assembly.
    /// </remarks>
    public static void ValidateInitialSetCompleteness(IReadOnlyDictionary<Type, Type> mapping)
    {
        if (mapping is null)
            throw new ArgumentNullException(nameof(mapping));

        // Spec-required families.
        // Some may not exist in the codebase yet; those are represented by placeholder checkers.
        // (We validate the presence of the checker types, not the fix provider types.)
        var requiredCheckerTypes = new HashSet<Type>
        {
            typeof(CollectionExpressionSafetyChecker),
            typeof(StringInterpolationSafetyChecker),
            typeof(PlaceholderSafetyChecker),
        };

        foreach (var required in requiredCheckerTypes)
        {
            var found = false;
            foreach (var entry in mapping)
            {
                if (entry.Value == required)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new InvalidOperationException($"Missing required safety checker in mapping: {required.FullName}");
        }
    }

    /// <summary>
    /// Validates that a raw mapping list does not contain duplicate fix provider types.
    /// </summary>
    /// <remarks>
    /// A dictionary cannot contain duplicate keys by construction, so this validation is only
    /// meaningful for list-based inputs.
    /// </remarks>
    public static void ValidateNoDuplicateFixProviderTypes(IEnumerable<KeyValuePair<Type, Type>> mapping)
    {
        if (mapping is null)
            throw new ArgumentNullException(nameof(mapping));

        var seen = new HashSet<Type>();
        foreach (var entry in mapping)
        {
            if (!seen.Add(entry.Key))
                throw new InvalidOperationException($"Duplicate fix provider type in mapping: {entry.Key.FullName}");
        }
    }

    public static void ValidateAllKnownFixProvidersAreMapped(IReadOnlyDictionary<Type, Type> mapping)
    {
        if (mapping is null)
            throw new ArgumentNullException(nameof(mapping));

        // Current codebase policy: only enforce mapping completeness for the fix providers
        // that are already part of the initial safety-checker rollout.
        // (A full assembly scan can be added later once all fix providers are migrated.)
        var requiredFixProviderTypes = new HashSet<Type>
        {
            typeof(Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider),
            typeof(Sharpen.Analyzer.FixProvider.CSharp10.UseInterpolatedStringCodeFixProvider),
        };

        foreach (var required in requiredFixProviderTypes)
        {
            if (!mapping.ContainsKey(required))
                throw new InvalidOperationException($"Missing required fix provider mapping: {required.FullName}");
        }
    }
}
