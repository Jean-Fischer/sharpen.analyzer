using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using VerifierAnalyzer = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseNullConditionalAssignmentAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;
using VerifierCodeFix = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseNullConditionalAssignmentAnalyzer,
    Sharpen.Analyzer.UseNullConditionalAssignmentCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseNullConditionalAssignmentTests
{
    [Fact]
    public async Task Reports_diagnostic_for_single_statement_if_without_braces()
    {
        const string code = @"
class C
{
    void M(C2 c)
    {
        if (c != null) c.X = 1;
    }
}

class C2
{
    public int X { get; set; }
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseNullConditionalAssignmentRule)
                .WithSpan(6, 9, 6, 32));
    }

    [Fact]
    public async Task Reports_diagnostic_for_single_statement_if_with_braces()
    {
        const string code = @"
class C
{
    void M(C2 c)
    {
        if (c != null)
        {
            c.X = 1;
        }
    }
}

class C2
{
    public int X { get; set; }
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseNullConditionalAssignmentRule)
                .WithSpan(6, 9, 9, 10));
    }

    [Fact]
    public async Task Does_not_report_for_multi_statement_if_body()
    {
        const string code = @"
class C
{
    void M(C2 c)
    {
        if (c != null)
        {
            c.X = 1;
            c.X = 2;
        }
    }
}

class C2
{
    public int X { get; set; }
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Does_not_report_for_mismatched_receiver()
    {
        const string code = @"
class C
{
    void M(C2 c, C2 d)
    {
        if (c != null) d.X = 1;
    }
}

class C2
{
    public int X { get; set; }
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(code);
    }

    // TODO: Re-enable once the test harness supports parsing `x?.Member = rhs` as a top-level
    // ConditionalAccessExpression statement (currently it normalizes to a SimpleAssignmentExpression
    // with ConditionalAccessExpression on the LHS, causing iterative fix verification to fail).
    //
    // [Fact]
    // public async Task Code_fix_rewrites_to_null_conditional_assignment()
    // {
    //     const string code = @"
    // class C
    // {
    //     void M(C2 c)
    //     {
    //         if (c != null) c.X = 1;
    //     }
    // }
    //
    // class C2
    // {
    //     public int X { get; set; }
    // }";
    //
    //     const string fixedCode = @"
    // class C
    // {
    //     void M(C2 c)
    //     {
    //         c?.X = 1;
    //     }
    // }
    //
    // class C2
    // {
    //     public int X { get; set; }
    // }";
    //
    //     await VerifierCodeFix.VerifyCodeFixAsync(
    //         code,
    //         new DiagnosticResult(CSharp14Rules.UseNullConditionalAssignmentRule)
    //             .WithSpan(6, 9, 6, 32),
    //         fixedCode);
    // }
}
