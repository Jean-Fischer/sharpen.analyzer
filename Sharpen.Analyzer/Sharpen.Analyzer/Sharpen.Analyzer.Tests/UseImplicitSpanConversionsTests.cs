using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using VerifierCodeFix = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseImplicitSpanConversionsAnalyzer,
    Sharpen.Analyzer.UseImplicitSpanConversionsCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseImplicitSpanConversionsTests
{
    [Fact]
    public async Task Analyzer_ReportsDiagnostic_ForArrayAsSpanPassedToReadOnlySpanParameter()
    {
        var code =
            "using System;\r\n\r\nclass C\r\n{\r\n    void M(ReadOnlySpan<int> s) { }\r\n\r\n    void Test(int[] a)\r\n    {\r\n        M(a.AsSpan());\r\n    }\r\n}";

        await VerifierCodeFix.VerifyCodeFixAsync(
            code,
            new DiagnosticResult(CSharp14Rules.UseImplicitSpanConversionsRule)
                .WithSpan(9, 11, 9, 21),
            "using System;\r\n\r\nclass C\r\n{\r\n    void M(ReadOnlySpan<int> s) { }\r\n\r\n    void Test(int[] a)\r\n    {\r\n        M(a);\r\n    }\r\n}");
    }

    [Fact]
    public async Task Analyzer_DoesNotReport_WhenOverloadResolutionWouldChange()
    {
        var code =
            "using System;\r\n\r\nclass C\r\n{\r\n    void M(int[] a) { }\r\n    void M(ReadOnlySpan<int> s) { }\r\n\r\n    void Test(int[] a)\r\n    {\r\n        M(a.AsSpan());\r\n    }\r\n}";

        await VerifierCodeFix.VerifyCodeFixAsync(code, DiagnosticResult.EmptyDiagnosticResults, code);
    }
}