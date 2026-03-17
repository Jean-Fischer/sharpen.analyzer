using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseFromEndIndexInObjectInitializersAnalyzer,
    Sharpen.Analyzer.UseFromEndIndexInObjectInitializersCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseFromEndIndexInObjectInitializersCodeFixTests
{
    [Fact]
    public async Task When_LengthMinusOneInInitializer_Then_FixRewritesToFromEndIndex()
    {
        const string code = @"
class C
{
    void M()
    {
        var a = new int[10];
        var c = new C { [a.Length - 1] = 42 };
    }

    public int this[System.Index i]
    {
        get => 0;
        set { }
    }
}";

        const string fixedCode = @"
class C
{
    void M()
    {
        var a = new int[10];
        var c = new C { [^1] = 42 };
    }

    public int this[System.Index i]
    {
        get => 0;
        set { }
    }
}";

        var expected = Verifier.Diagnostic(CSharp13Rules.UseFromEndIndexInObjectInitializersRule)
            .WithLocation(7, 26);

        await Verifier.VerifyCodeFixAsync(code, expected, fixedCode);
    }

    [Fact]
    public async Task When_TargetIsNotArray_Then_NoDiagnosticAndNoFix()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""hello"";
        var c = new C { [s.Length - 1] = 42 };
    }

    public int this[System.Index i]
    {
        get => 0;
        set { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}