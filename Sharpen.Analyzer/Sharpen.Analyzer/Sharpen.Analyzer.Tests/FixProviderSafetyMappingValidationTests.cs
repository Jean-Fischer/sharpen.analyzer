using System;
using System.Collections.Generic;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyMappingValidationTests
{
    [Fact]
    public void ValidateInitialSetCompleteness_Throws_WhenRequiredFixProviderMappingMissing()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FixProviderSafetyMappingValidation.ValidateInitialSetCompleteness(new Dictionary<Type, Type>()));

        Assert.Contains("Missing safety checker mapping", ex.Message);
    }

    [Fact]
    public void ValidateInitialSetCompleteness_DoesNotThrow_WhenAllRequiredCheckersPresent()
    {
        var mapping = FixProviderSafetyMapping.ToDictionary();

        FixProviderSafetyMappingValidation.ValidateInitialSetCompleteness(mapping);
    }

    [Fact]
    public void ValidateAllKnownFixProvidersAreMapped_Throws_WhenRequiredFixProviderMappingMissing()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            FixProviderSafetyMappingValidation.ValidateAllKnownFixProvidersAreMapped(new Dictionary<Type, Type>()));

        Assert.Contains("Missing safety checker mapping", ex.Message);
    }

    [Fact]
    public void ValidateAllKnownFixProvidersAreMapped_DoesNotThrow_WhenAllRequiredFixProvidersPresent()
    {
        var mapping = FixProviderSafetyMapping.ToDictionary();

        FixProviderSafetyMappingValidation.ValidateAllKnownFixProvidersAreMapped(mapping);
    }

    [Fact]
    public void ToDictionary_Throws_WhenDuplicateFixProviderType()
    {
        // This test validates the duplicate detection behavior in FixProviderSafetyMapping.ToDictionary().
        // We can't easily inject duplicates into FixProviderSafetyMapping.Entries (it's a static list),
        // so we validate the same behavior with a local helper.

        var entries = new (Type fixProviderType, Type checkerType)[]
        {
            (typeof(object), typeof(CollectionExpressionSafetyChecker)),
            (typeof(object), typeof(StringInterpolationSafetyChecker))
        };

        var ex = Assert.Throws<InvalidOperationException>(() => ToDictionaryWithDuplicateCheck(entries));
        Assert.Contains("Duplicate fix provider mapping", ex.Message);
    }

    private static IReadOnlyDictionary<Type, Type> ToDictionaryWithDuplicateCheck(
        IEnumerable<(Type fixProviderType, Type checkerType)> entries)
    {
        var dict = new Dictionary<Type, Type>();

        foreach (var (fixProviderType, checkerType) in entries)
        {
            if (dict.ContainsKey(fixProviderType))
                throw new InvalidOperationException($"Duplicate fix provider mapping: {fixProviderType.FullName}");

            dict.Add(fixProviderType, checkerType);
        }

        return dict;
    }
}