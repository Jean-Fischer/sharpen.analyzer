using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

/// <summary>
/// Canonical mapping between fix providers and their dedicated safety checkers.
/// </summary>
public static class FixProviderSafetyMapping
{
    /// <summary>
    /// Returns the canonical mapping table.
    /// </summary>
    /// <remarks>
    /// This mapping must not introduce a compile-time dependency from the analyzer assembly to the fix-provider assembly.
    ///
    /// We therefore resolve fix provider types by name at runtime, scanning already-loaded assemblies only.
    /// (No <c>Assembly.Load</c> calls; analyzer assemblies are subject to RS1035.)
    /// </remarks>
    public static ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)> Entries { get; } =
        CreateEntries();

    public static IReadOnlyDictionary<Type, Type> ToDictionary()
    {
        var dict = new Dictionary<Type, Type>();

        foreach (var (fixProviderType, safetyCheckerType) in Entries)
        {
            if (dict.ContainsKey(fixProviderType))
                throw new InvalidOperationException($"Duplicate fix provider mapping: {fixProviderType.FullName}");

            dict.Add(fixProviderType, safetyCheckerType);
        }

        return dict;
    }

    public static Type GetSafetyCheckerType(Type fixProviderType)
    {
        if (fixProviderType is null)
            throw new ArgumentNullException(nameof(fixProviderType));

        var dict = ToDictionary();
        if (!dict.TryGetValue(fixProviderType, out var checkerType))
            throw new InvalidOperationException($"No safety checker mapping found for fix provider: {fixProviderType.FullName}");

        return checkerType;
    }

    private static ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)> CreateEntries()
    {
        var builder = ImmutableArray.CreateBuilder<(Type FixProviderType, Type SafetyCheckerType)>();

        // CSharp12: UseCollectionExpression
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider",
            safetyCheckerType: typeof(CollectionExpressionSafetyChecker));

        // CSharp13: PreferParamsCollections
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.PreferParamsCollectionsCodeFixProvider",
            safetyCheckerType: typeof(PreferParamsCollectionsSafetyChecker));

        // CSharp13: UseFromEndIndexInObjectInitializers
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseFromEndIndexInObjectInitializersCodeFixProvider",
            safetyCheckerType: typeof(UseFromEndIndexInObjectInitializersSafetyChecker));

        // CSharp13: UseEscapeSequenceE
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseEscapeSequenceECodeFixProvider",
            safetyCheckerType: typeof(UseEscapeSequenceESafetyChecker));

        // CSharp13: UseSystemThreadingLock
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseSystemThreadingLockCodeFixProvider",
            safetyCheckerType: typeof(UseSystemThreadingLockSafetyChecker));

        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.PartialPropertiesIndexersRefactoringCodeFixProvider",
            safetyCheckerType: typeof(PartialPropertiesIndexersRefactoringSafetyChecker));

        // CSharp14: Field-backed properties
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseFieldKeywordInPropertiesCodeFixProvider",
            safetyCheckerType: typeof(FieldBackedPropertySafetyChecker));

        // CSharp14: Null-conditional assignment
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseNullConditionalAssignmentCodeFixProvider",
            safetyCheckerType: typeof(NullConditionalAssignmentSafetyChecker));

        // CSharp14: nameof with unbound generic types
        AddIfResolved(
            builder,
            fixProviderFullName: "Sharpen.Analyzer.UseUnboundGenericTypeInNameofCodeFixProvider",
            safetyCheckerType: typeof(UnboundGenericTypeInNameofSafetyChecker));

        return builder.ToImmutable();
    }

    private static void AddIfResolved(
        ImmutableArray<(Type FixProviderType, Type SafetyCheckerType)>.Builder builder,
        string fixProviderFullName,
        Type safetyCheckerType,
        string? preferredAssemblyName = null)
    {
        var fixProviderType = FixProviderSafetyTypeResolution.ResolveType(fixProviderFullName, preferredAssemblyName);
        if (fixProviderType is null)
            return;

        builder.Add((fixProviderType, safetyCheckerType));
    }

    public static void ValidateUniqueness()
    {
        var duplicates = Entries
            .GroupBy(e => e.FixProviderType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            var msg = "Duplicate fix provider mapping entries: " + string.Join(", ", duplicates.Select(t => t.FullName));
            throw new InvalidOperationException(msg);
        }
    }
}
