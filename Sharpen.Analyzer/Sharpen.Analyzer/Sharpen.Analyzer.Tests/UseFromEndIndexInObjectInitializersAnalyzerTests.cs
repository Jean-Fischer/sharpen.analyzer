using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseFromEndIndexInObjectInitializersAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseFromEndIndexInObjectInitializersAnalyzerTests
{
    [Fact]
    public async Task When_LengthMinusOneInInitializer_Then_DiagnosticIsReported()
    {
        const string code = @"
class C
{
    void M()
    {
        var a = new int[10];
        var c = new C { [a.Length - 1] = 42 };
    }

    public int this[int i]
    {
        get => 0;
        set { }
    }
}";

        var expected = Verifier.Diagnostic(CSharp13Rules.UseFromEndIndexInObjectInitializersRule)
            .WithLocation(7, 26);

        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task When_ConstantIndexInInitializer_Then_NoDiagnostic()
    {
        const string code = @"
class C
{
    void M()
    {
        var a = new int[10];
        var c = new C { [0] = 42 };
    }

    public int this[int i]
    {
        get => 0;
        set { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
