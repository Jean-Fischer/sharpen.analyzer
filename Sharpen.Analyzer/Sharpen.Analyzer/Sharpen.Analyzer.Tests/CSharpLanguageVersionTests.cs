using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sharpen.Analyzer.Common;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public class CSharpLanguageVersionTests
{
    [Fact]
    public async Task IsCSharp9OrAbove_ReturnsFalse_ForCSharp8()
    {
        var compilation = await CreateCompilationAsync(LanguageVersion.CSharp8).ConfigureAwait(false);
        Assert.False(CSharpLanguageVersion.IsCSharp9OrAbove(compilation));
    }

    [Fact]
    public async Task IsCSharp9OrAbove_ReturnsTrue_ForCSharp9()
    {
        var compilation = await CreateCompilationAsync(LanguageVersion.CSharp9).ConfigureAwait(false);
        Assert.True(CSharpLanguageVersion.IsCSharp9OrAbove(compilation));
    }

    private static async Task<Compilation> CreateCompilationAsync(LanguageVersion languageVersion)
    {
        var parseOptions = new CSharpParseOptions(languageVersion);
        var document = new AdhocWorkspace()
            .AddProject("TestProject", LanguageNames.CSharp)
            .WithParseOptions(parseOptions)
            .AddDocument("Test.cs", "public class C { }");

        return await document.Project.GetCompilationAsync().ConfigureAwait(false)
               ?? throw new Xunit.Sdk.XunitException("Failed to create compilation");
    }
}
