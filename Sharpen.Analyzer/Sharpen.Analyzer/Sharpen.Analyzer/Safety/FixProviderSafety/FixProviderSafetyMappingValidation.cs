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
}
