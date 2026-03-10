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
        var compilation = await CreateCompilationAsync(LanguageVersion.CSharp8);
        Assert.False(CSharpLanguageVersion.IsCSharp9OrAbove(compilation));
    }

    [Fact]
    public async Task IsCSharp9OrAbove_ReturnsTrue_ForCSharp9()
    {
        var compilation = await CreateCompilationAsync(LanguageVersion.CSharp9);
        Assert.True(CSharpLanguageVersion.IsCSharp9OrAbove(compilation));
    }

    [Fact]
    public async Task IsCSharp11OrAbove_ReturnsFalse_ForCSharp10()
    {
        var compilation = await CreateCompilationAsync(LanguageVersion.CSharp10);
        Assert.False(CSharpLanguageVersion.IsCSharp11OrAbove(compilation));
    }

    [Fact]
    public async Task IsCSharp11OrAbove_ReturnsTrue_ForPreview()
    {
        // The Microsoft.CodeAnalysis version referenced by this project does not expose LanguageVersion.CSharp11.
        // Using Preview is the only reliable way to exercise the "11 or above" branch.
        var compilation = await CreateCompilationAsync(LanguageVersion.Preview);
        Assert.True(CSharpLanguageVersion.IsCSharp11OrAbove(compilation));
    }

    private static async Task<Compilation> CreateCompilationAsync(LanguageVersion languageVersion)
    {
        var parseOptions = new CSharpParseOptions(languageVersion);
        var document = new AdhocWorkspace()
            .AddProject("TestProject", LanguageNames.CSharp)
            .WithParseOptions(parseOptions)
            .AddDocument("Test.cs", "public class C { }");

        return await document.Project.GetCompilationAsync()
               ?? throw new Xunit.Sdk.XunitException("Failed to create compilation");
    }
}
