using System;
using System.Collections.Generic;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public static partial class FixProviderSafetyMappingValidation
{
    private static void ThrowIfNull<T>(T value) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
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
