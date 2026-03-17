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

        // This repository previously targeted a Roslyn version that did not expose LanguageVersion.CSharp11.
        // In that situation, the only reliable way to exercise "11+" behavior is via LanguageVersion.Preview.
        // Avoid relying on LanguageVersion enum integer values, which are not a stable contract.
        return csharpCompilation.LanguageVersion is LanguageVersion.CSharp11 or LanguageVersion.Preview;
    }

    public static bool IsCSharp12OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        // Treat Preview as "latest".
        return csharpCompilation.LanguageVersion is LanguageVersion.CSharp12 or LanguageVersion.Preview;
    }

    public static bool IsCSharp13OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        // Treat Preview as "latest".
        return csharpCompilation.LanguageVersion is LanguageVersion.CSharp13 or LanguageVersion.Preview;
    }

    public static bool IsCSharp14OrAbove(Compilation compilation)
    {
        if (compilation is not CSharpCompilation csharpCompilation)
            return false;

        // Treat Preview as "latest".
        return csharpCompilation.LanguageVersion is LanguageVersion.CSharp14 or LanguageVersion.Preview;
    }
}