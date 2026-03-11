using System;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

internal static class SafetyCheckHelpers
{
    public static FixProviderSafetyResult Unsafe(string reasonId, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(reasonId))
            throw new ArgumentException("ReasonId must be non-empty.", nameof(reasonId));

        return FixProviderSafetyResult.Unsafe(reasonId, message);
    }

    public static FixProviderSafetyResult Safe() => FixProviderSafetyResult.Safe();

    public static FixProviderSafetyResult UnsafeIfNull(object? value, string reasonId, string? message = null)
        => value is null ? Unsafe(reasonId, message) : Safe();

    public static FixProviderSafetyResult UnsafeIfNotCSharp(Document? document, string reasonId, string? message = null)
        => document?.Project.Language != LanguageNames.CSharp ? Unsafe(reasonId, message) : Safe();
}
