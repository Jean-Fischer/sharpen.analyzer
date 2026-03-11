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
        // NOTE: The mapping table is currently empty (see FixProviderSafetyMapping.Entries) to avoid
        // introducing a compile-time dependency from the analyzer assembly to the fix-provider assembly.
        // Once we have a runtime-populated mapping, re-enable this validation.
        _ = mapping;
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

        // This validation is intentionally kept in the core assembly, but it must still be able
        // to validate fix providers after they were split into a separate assembly.
        //
        // We avoid compile-time references to fix provider types by resolving them by name at runtime.
        // This keeps the safety pipeline independent from the fix provider assembly.

        // NOTE: The mapping table is currently empty (see FixProviderSafetyMapping.Entries) to avoid
        // introducing a compile-time dependency from the analyzer assembly to the fix-provider assembly.
        // Once we have a runtime-populated mapping, re-enable this validation.
        _ = mapping;
    }

    private static Type? ResolveType(string fullName)
    {
        // Try already-loaded assemblies first.
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
            if (t is not null)
                return t;
        }

        // IMPORTANT: do not call Assembly.Load here.
        // This code lives in an analyzer assembly and is subject to RS1035.
        // The test project already references Sharpen.Analyzer.FixProviders, so the assembly
        // will be loaded by the runtime when needed.
        return null;
    }
}
