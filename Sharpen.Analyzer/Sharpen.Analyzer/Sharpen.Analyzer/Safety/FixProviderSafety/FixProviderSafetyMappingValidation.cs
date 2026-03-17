using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public static partial class FixProviderSafetyMappingValidation
{
    /// <summary>
    ///     Validates mapping completeness for the spec-required initial set.
    /// </summary>
    /// <remarks>
    ///     This is intentionally scoped to the initial set described in the spec.
    ///     It does not attempt to cover all fix providers in the assembly.
    /// </remarks>
    public static void ValidateInitialSetCompleteness(IReadOnlyDictionary<Type, Type> mapping)
    {
        ThrowIfNull(mapping);

        // Spec-required families.
        // This is intentionally scoped to the initial set described in the spec.
        // It does not attempt to cover all fix providers in the assembly.

        // NOTE: We avoid compile-time references to fix provider types by resolving them by name at runtime.
        // This keeps the analyzer assembly independent from the fix-provider assembly.
        //
        // IMPORTANT: On some runners (notably CI with coverage), the fix-provider assembly may not be loaded yet.
        // Provide the preferred assembly name so type resolution can bind deterministically.
        const string fixProvidersAssemblyName = "Sharpen.Analyzer.FixProviders";

        // CSharp12: UseCollectionExpression
        EnsureMapped(mapping, "Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider", fixProvidersAssemblyName);

        // CSharp13: PreferParamsCollections
        EnsureMapped(mapping, "Sharpen.Analyzer.PreferParamsCollectionsCodeFixProvider", fixProvidersAssemblyName);
    }

    /// <summary>
    ///     Validates that a raw mapping list does not contain duplicate fix provider types.
    /// </summary>
    /// <remarks>
    ///     A dictionary cannot contain duplicate keys by construction, so this validation is only
    ///     meaningful for list-based inputs.
    /// </remarks>
    public static void ValidateNoDuplicateFixProviderTypes(IEnumerable<KeyValuePair<Type, Type>> mapping)
    {
        var keyValuePairs = mapping as KeyValuePair<Type, Type>[] ?? mapping.ToArray();
        ThrowIfNull(keyValuePairs);

        var seen = new HashSet<Type>();
        foreach (var entry in keyValuePairs)
            if (!seen.Add(entry.Key))
                throw new InvalidOperationException($"Duplicate fix provider type in mapping: {entry.Key.FullName}");
    }

    public static void ValidateAllKnownFixProvidersAreMapped(IReadOnlyDictionary<Type, Type> mapping)
    {
        ThrowIfNull(mapping);

        // This validation is intentionally kept in the core assembly, but it must still be able
        // to validate fix providers after they were split into a separate assembly.
        //
        // We avoid compile-time references to fix provider types by resolving them by name at runtime.
        // This keeps the safety pipeline independent from the fix provider assembly.

        // For now, this is equivalent to the spec-required initial set.
        ValidateInitialSetCompleteness(mapping);
    }
}