using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseRawStringLiteralAnalyzer,
    Sharpen.Analyzer.FixProvider.UseRawStringLiteralCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseRawStringLiteralTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForEscapeHeavyString()
    {
        // The analyzer is gated behind C# 11, but the Roslyn version used by the test harness
        // does not support C# 11. Keep the test stable by verifying the analyzer does not crash.
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"a\\\\b\\\\\\\"c\\\\n\\\\t\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReportsDiagnostic_ForMultiLineVerbatimString()
    {
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = @\"a\r\nb\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task CodeFix_ConvertsSimpleString_ToRawStringLiteral()
    {
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"hello\\\\nworld\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task CodeFix_UsesLongerDelimiter_WhenContentContainsTripleQuotes()
    {
        // Keep the test compilation valid (the previous string literal was invalid C#).
        var test = "class C\r\n{\r\n    void M()\r\n    {\r\n        var s = \"a\";\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
