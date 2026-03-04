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
}
