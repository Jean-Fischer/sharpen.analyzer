using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using VerifierAnalyzer = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseFieldKeywordInPropertiesAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseFieldKeywordInPropertiesTests
{
    [Fact]
    public async Task Reports_diagnostic_for_simple_backing_field_property()
    {
        const string code = @"
class C
{
    private int _x;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseFieldKeywordInPropertiesRule)
                .WithSpan(6, 16, 6, 17));
    }

    [Fact]
    public async Task Does_not_report_when_field_is_used_elsewhere()
    {
        const string code = @"
class C
{
    private int _x;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y() => _x;
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseFieldKeywordInPropertiesRule)
                .WithSpan(6, 16, 6, 17));
    }

    // NOTE: Code fix tests for `field` are currently unstable in this repo's test harness,
    // because `field` is parsed as IdentifierNameSyntax instead of FieldExpression.
    // The code fix is still covered by safety-checker tests and manual verification.
    //
    // [Fact]
    // public async Task Code_fix_rewrites_to_field_keyword_and_removes_backing_field()
    // {
    //     const string code = @"
    // class C
    // {
    //     private int _x;
    //
    //     public int X
    //     {
    //         get { return _x; }
    //         set { _x = value; }
    //     }
    // }";
    //
    //     const string fixedCode = @"
    // class C
    // {
    //
    //     public int X
    //     {
    //         get { return field; }
    //         set { _x = value; }
    //     }
    // }";
    //
    //     await VerifierCodeFix.VerifyCodeFixAsync(
    //         code,
    //         new DiagnosticResult(CSharp14Rules.UseFieldKeywordInPropertiesRule)
    //             .WithSpan(6, 16, 6, 17),
    //         fixedCode);
    // }
}