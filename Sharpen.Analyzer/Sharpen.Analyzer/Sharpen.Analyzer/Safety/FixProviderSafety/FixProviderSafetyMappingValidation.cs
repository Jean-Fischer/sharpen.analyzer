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
        // This is intentionally scoped to the initial set described in the spec.
        // It does not attempt to cover all fix providers in the assembly.

        // NOTE: We avoid compile-time references to fix provider types by resolving them by name at runtime.
        // This keeps the analyzer assembly independent from the fix-provider assembly.

        // CSharp12: UseCollectionExpression
        EnsureMapped(mapping, "Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider");
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

        // For now, this is equivalent to the spec-required initial set.
        ValidateInitialSetCompleteness(mapping);
    }

    private static void EnsureMapped(IReadOnlyDictionary<Type, Type> mapping, string fixProviderFullName, string? preferredAssemblyName = null)
    {
        var fixProviderType = ResolveType(fixProviderFullName, preferredAssemblyName);
        if (fixProviderType is null)
            throw new InvalidOperationException($"Fix provider type could not be resolved (assembly not loaded?): {fixProviderFullName}");

        if (!mapping.ContainsKey(fixProviderType))
            throw new InvalidOperationException($"Missing safety checker mapping for fix provider: {fixProviderType.FullName}");
    }

    private static Type? ResolveType(string fullName, string? preferredAssemblyName = null)
    {
        // Try already-loaded assemblies first.
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (preferredAssemblyName is not null)
            {
                var asmName = asm.GetName().Name;
                if (!string.Equals(asmName, preferredAssemblyName, StringComparison.Ordinal))
                    continue;
            }

            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
            if (t is not null)
                return t;
        }

        if (preferredAssemblyName is not null)
        {
            // Fallback: scan all already-loaded assemblies.
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (t is not null)
                    return t;
            }
        }

        // IMPORTANT: do not call Assembly.Load here.
        // This code lives in an analyzer assembly and is subject to RS1035.
        // The test project already references Sharpen.Analyzer.FixProviders, so the assembly
        // will be loaded by the runtime when needed.
        return null;
    }
}
