using System;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

internal static class FixProviderSafetyTypeResolution
{
    internal static Type? ResolveType(string fullName, string? preferredAssemblyName = null)
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

        // If the fix provider assembly isn't loaded yet, try to force-load it in a safe way.
        //
        // Rationale:
        // - In some test/coverage runners, a referenced assembly may not be loaded until a type is touched.
        // - We must not call Assembly.Load here (analyzer assemblies are subject to RS1035).
        // - Type.GetType("Namespace.Type, AssemblyName") triggers normal type resolution without explicit loads.
        //
        // This keeps production behavior unchanged when the assembly is already loaded, while making
        // validation robust in CI/coverage runs.
        var assemblyQualifiedName = preferredAssemblyName is null
            ? fullName
            : $"{fullName}, {preferredAssemblyName}";

        return Type.GetType(assemblyQualifiedName, throwOnError: false, ignoreCase: false);
    }
}
