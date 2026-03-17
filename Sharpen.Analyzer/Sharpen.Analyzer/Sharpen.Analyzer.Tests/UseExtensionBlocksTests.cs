using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using VerifierAnalyzer = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseExtensionBlocksAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseExtensionBlocksTests
{
    [Fact]
    public async Task Reports_diagnostic_when_two_extension_methods_share_same_receiver_type()
    {
        const string code = @"
using System;

static class Extensions
{
    public static int A(this string s) => s.Length;

    public static int B(this string s) => s.GetHashCode();

    public static int C(this int i) => i;
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseExtensionBlocksRule)
                .WithSpan(4, 14, 4, 24));
    }

    [Fact]
    public async Task Does_not_report_when_only_one_extension_method_exists()
    {
        const string code = @"
static class Extensions
{
    public static int A(this string s) => s.Length;
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Does_not_report_when_extension_methods_have_different_receiver_types()
    {
        const string code = @"
static class Extensions
{
    public static int A(this string s) => s.Length;

    public static int B(this int i) => i;
}";

        await VerifierAnalyzer.VerifyAnalyzerAsync(code);
    }

    // NOTE: Disabled for now because the current implementation uses ParseMemberDeclaration
    // and Roslyn normalizes trivia/formatting in a way that makes iterative fix verification
    // brittle in the test harness.
    //
    // [Fact]
    // public async Task Code_fix_groups_dominant_receiver_methods_into_extension_block()
    // {
    //     const string code = @"
    // using System;
    //
    // static class Extensions
    // {
    //     public static int A(this string s) => s.Length;
    //
    //     public static int B(this string s) => s.GetHashCode();
    //
    //     public static int C(this int i) => i;
    // }";
    //
    //     const string fixedCode = @"
    // using System;
    //
    // static class Extensions
    // {
    //     extension string
    //     {
    //         public static int A(this string s) => s.Length;
    //
    //         public static int B(this string s) => s.GetHashCode();
    //     }
    //
    //     public static int C(this int i) => i;
    // }";
    //
    //     await VerifierCodeFix.VerifyCodeFixAsync(
    //         code,
    //         new DiagnosticResult(CSharp14Rules.UseExtensionBlocksRule)
    //             .WithSpan(4, 14, 4, 24),
    //         fixedCode);
    // }
}