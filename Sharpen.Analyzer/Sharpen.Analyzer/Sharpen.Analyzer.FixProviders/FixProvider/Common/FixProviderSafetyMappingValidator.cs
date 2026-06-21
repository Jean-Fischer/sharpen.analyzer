using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class FixProviderSafetyMappingValidator
{
    private static readonly Lazy<ImmutableArray<(Type ProviderType, Type CheckerType)>> Mappings =
        new(BuildMappings);

    public static void EnsureValidated()
    {
        _ = Mappings.Value;
    }

    private static ImmutableArray<(Type ProviderType, Type CheckerType)> BuildMappings()
    {
        var fixProvidersAssembly = typeof(SafetyCheckedCodeFixRegistration).Assembly;
        var mappings = fixProvidersAssembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(CodeFixProvider).IsAssignableFrom(type))
            .Select(type => (ProviderType: type, CheckerType: TryGetCheckerType(type)))
            .Where(pair => pair.CheckerType is not null)
            .Select(pair => (ProviderType: pair.ProviderType, CheckerType: pair.CheckerType!))
            .ToImmutableArray();

        var duplicateCheckerGroups = mappings
            .GroupBy(mapping => mapping.CheckerType)
            .Where(group => group.Count() > 1)
            .ToArray();

        if (duplicateCheckerGroups.Length > 0)
        {
            var message = string.Join(
                Environment.NewLine,
                duplicateCheckerGroups.Select(group =>
                    $"- {group.Key.FullName}: {string.Join(", ", group.Select(mapping => mapping.ProviderType.FullName))}"));

            throw new InvalidOperationException(
                $"Duplicate safety checker mappings detected:{Environment.NewLine}{message}");
        }

        return mappings;
    }

    private static Type? TryGetCheckerType(Type type)
    {
        for (var current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            if (!current.IsGenericType)
                continue;

            var genericTypeDefinition = current.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(SafetyCheckedSharpenCodeFixProvider<,>) ||
                genericTypeDefinition == typeof(CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<,>))
            {
                return current.GetGenericArguments()[1];
            }
        }

        return null;
    }
}
