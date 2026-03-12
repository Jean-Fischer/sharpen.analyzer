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

        // IMPORTANT: do not call Assembly.Load here.
        // This code lives in an analyzer assembly and is subject to RS1035.
        // The test project already references Sharpen.Analyzer.FixProviders, so the assembly
        // will be loaded by the runtime when needed.
        return null;
    }
}
