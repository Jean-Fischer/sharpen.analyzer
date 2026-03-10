using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseInterpolatedStringAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseInterpolatedStringCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseInterpolatedStringTests
{
    [Fact]
    public async Task UseInterpolatedString_TriggersAndFixes_ForStringFormat()
    {
        var source = @"
class C
{
    string M(string name)
    {
        return string.Format(""Hello, {0}!"", name);
    }
}
";

        var fixedSource = @"
class C
{
    string M(string name)
    {
        return $""Hello, {name}!"";
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            source,
            new[]
            {
                Verifier.Diagnostic("SHARPEN044").WithSpan(6, 16, 6, 50).WithSeverity(DiagnosticSeverity.Info)
            });
    }

    [Fact]
    public async Task UseInterpolatedString_TriggersAndFixes_ForConcatenationChain()
    {
        var source = @"
class C
{
    string M(string name)
    {
        return ""Hello, "" + name + ""!"";
    }
}
";

        var fixedSource = @"
class C
{
    string M(string name)
    {
        return $""Hello, {name}!"";
    }
}
";

        await Verifier.VerifyCodeFixAsync(
            source,
            new[]
            {
                Verifier.Diagnostic("SHARPEN044").WithSpan(6, 16, 6, 38).WithSeverity(DiagnosticSeverity.Info)
            },
            fixedSource);
    }

    [Fact]
    public async Task UseInterpolatedString_TriggersAndFixes_ForConstConcatenation()
    {
        var source = @"
class C
{
    const string Prefix = ""Hello"";

    const string S = Prefix + ""!"";
}
";

        var fixedSource = @"
class C
{
    const string Prefix = ""Hello"";

    const string S = $""{Prefix}!"";
}
";

        var expected = Verifier.Diagnostic("SHARPEN045").WithSpan(6, 22, 6, 34).WithSeverity(DiagnosticSeverity.Info);
        await Verifier.VerifyCodeFixAsync(source, expected, fixedSource);
    }
}
