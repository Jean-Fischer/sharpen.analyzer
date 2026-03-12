using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sharpen.Analyzer.FixProvider.Common;

internal static class CSharpLanguageVersion
{
    internal static bool IsCSharp9OrAbove(Compilation compilation) =>
        compilation is not null && compilation.LanguageVersion() >= 9;

    internal static bool IsCSharp10OrAbove(Compilation compilation) =>
        compilation is not null && compilation.LanguageVersion() >= 10;

    internal static bool IsCSharp11OrAbove(Compilation compilation) =>
        compilation is not null && compilation.LanguageVersion() >= 11;

    internal static bool IsCSharp12OrAbove(Compilation compilation) =>
        compilation is not null && compilation.LanguageVersion() >= 12;

    private static int LanguageVersion(this Compilation compilation)
    {
        // Keep this helper self-contained in FixProviders assembly.
        // We infer the language version from the parse options of the first syntax tree.
        // If unavailable, default to 0 (unknown/too old).
        var options = compilation.SyntaxTrees.FirstOrDefault()?.Options as CSharpParseOptions;
        return options?.LanguageVersion is LanguageVersion v
            ? (int)v
            : 0;
    }
}
