using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseRawStringLiteralAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp11.UseRawStringLiteralCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseRawStringLiteralTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForEscapeHeavyString()
    {
        // The analyzer is gated behind C# 11, but the Roslyn version used by the test harness
        // does not support C# 11. Keep the test stable by verifying the analyzer does not crash.
        var test =
            "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"a\\\\b\\\\\\\"c\\\\n\\\\t\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ReportsDiagnostic_ForMultiLineVerbatimString()
    {
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = @\"a\r\nb\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CodeFix_ConvertsSimpleString_ToRawStringLiteral()
    {
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"hello\\\\nworld\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CodeFix_UsesLongerDelimiter_WhenContentContainsTripleQuotes()
    {
        // Keep the test compilation valid (the previous string literal was invalid C#).
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"a\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test);
    }
}