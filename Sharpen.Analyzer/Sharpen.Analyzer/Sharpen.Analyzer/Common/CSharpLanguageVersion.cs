using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sharpen.Analyzer.Common;

public static class CSharpLanguageVersion
{
    public static bool IsCSharp9OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        return csharpCompilation.LanguageVersion >= LanguageVersion.CSharp9;
    }

    public static bool IsCSharp10OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        return csharpCompilation.LanguageVersion >= LanguageVersion.CSharp10;
    }

    public static bool IsCSharp11OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        // This repository currently targets a Roslyn version that does not expose LanguageVersion.CSharp11.
        // In that situation, the only reliable way to exercise "11+" behavior is via LanguageVersion.Preview.
        // Avoid relying on LanguageVersion enum integer values, which are not a stable contract.
        return csharpCompilation.LanguageVersion == LanguageVersion.Preview;
    }
}
