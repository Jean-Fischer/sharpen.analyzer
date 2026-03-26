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

    private static void EnsureMapped(IReadOnlyDictionary<Type, Type> mapping, string fixProviderFullName,
        string? preferredAssemblyName = null)
    {
        var fixProviderType = FixProviderSafetyTypeResolution.ResolveType(fixProviderFullName, preferredAssemblyName);
        if (fixProviderType is null)
        {
            throw new InvalidOperationException(
                $"Fix provider type could not be resolved (assembly not loaded?): {fixProviderFullName}");
        }

        if (!mapping.ContainsKey(fixProviderType))
        {
            throw new InvalidOperationException(
                $"Missing safety checker mapping for fix provider: {fixProviderType.FullName}");
        }
    }
}