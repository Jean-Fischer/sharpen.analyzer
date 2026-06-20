using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.FixProvider.CSharp10;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyContractTests
{
    [Fact]
    public void SafetyCheckedProviders_MatchTheExpectedSet()
    {
        var expected = new HashSet<string>(StringComparer.Ordinal)
        {
            "Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider",
            "Sharpen.Analyzer.PreferParamsCollectionsCodeFixProvider",
            "Sharpen.Analyzer.UseEscapeSequenceECodeFixProvider",
            "Sharpen.Analyzer.UseFromEndIndexInObjectInitializersCodeFixProvider",
            "Sharpen.Analyzer.UseSystemThreadingLockCodeFixProvider",
            "Sharpen.Analyzer.PartialPropertiesIndexersRefactoringCodeFixProvider",
            "Sharpen.Analyzer.UseExtensionBlocksCodeFixProvider",
            "Sharpen.Analyzer.UseFieldKeywordInPropertiesCodeFixProvider",
            "Sharpen.Analyzer.UseImplicitSpanConversionsCodeFixProvider",
            "Sharpen.Analyzer.UseLambdaParameterModifiersWithoutTypesCodeFixProvider",
            "Sharpen.Analyzer.UseNullConditionalAssignmentCodeFixProvider",
            "Sharpen.Analyzer.UseUnboundGenericTypeInNameofCodeFixProvider",
            "Sharpen.Analyzer.FixProvider.CSharp10.UseInterpolatedStringCodeFixProvider"
        };

        var actual = GetSafetyCheckedProviders()
            .Select(static type => type.FullName!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SafetyCheckedProviders_ExposeConcreteSafetyCheckers()
    {
        foreach (var providerType in GetSafetyCheckedProviders())
        {
            var checkerType = GetSafetyCheckerType(providerType);

            Assert.True(typeof(IFixProviderSafetyChecker).IsAssignableFrom(checkerType));
            Assert.False(checkerType.IsAbstract);
            Assert.NotNull(checkerType.GetConstructor(Type.EmptyTypes));
        }
    }

    [Fact]
    public void SafetyCheckedProviders_UseUniqueSafetyCheckers()
    {
        var duplicateCheckerGroups = GetSafetyCheckedProviders()
            .Select(providerType => new
            {
                Provider = providerType,
                Checker = GetSafetyCheckerType(providerType)
            })
            .GroupBy(entry => entry.Checker)
            .Where(group => group.Count() > 1)
            .ToArray();

        Assert.Empty(duplicateCheckerGroups);
    }

    [Fact]
    public void SafetyCheckedProviders_AreRealCodeFixProviders()
    {
        foreach (var providerType in GetSafetyCheckedProviders())
            Assert.True(typeof(CodeFixProvider).IsAssignableFrom(providerType));
    }

    private static Type[] GetSafetyCheckedProviders()
    {
        return typeof(UseInterpolatedStringCodeFixProvider).Assembly
            .GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true })
            .Where(type => typeof(CodeFixProvider).IsAssignableFrom(type))
            .Where(IsSafetyCheckedProviderType)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsSafetyCheckedProviderType(Type type)
    {
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType!)
        {
            if (!current.IsGenericType)
                continue;

            var genericTypeDefinition = current.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(SafetyCheckedSharpenCodeFixProvider<,>) ||
                genericTypeDefinition == typeof(CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<,>))
            {
                return true;
            }
        }

        return false;
    }

    private static Type GetSafetyCheckerType(Type providerType)
    {
        for (var current = providerType; current is not null && current != typeof(object); current = current.BaseType!)
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

        throw new InvalidOperationException($"Provider '{providerType.FullName}' does not use the shared safety contract.");
    }
}
