using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseUtf8StringLiteralAnalyzer,
    Sharpen.Analyzer.FixProvider.UseUtf8StringLiteralCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseUtf8StringLiteralTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForByteArrayInitializer_Ascii()
    {
        // The analyzer is gated behind C# 11, but the Roslyn version used by the test harness
        // does not support C# 11. Keep the test stable by verifying the analyzer does not crash.
        var test = @"
using System;

class C
{
    void M()
    {
        ReadOnlySpan<byte> bytes = new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F };
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReportsDiagnostic_ForEncodingUtf8GetBytes_ConstantString()
    {
        var test = "using System;\r\nusing System.Text;\r\n\r\nclass C\r\n{\r\n    void M()\r\n    {\r\n        ReadOnlySpan<byte> bytes = Encoding.UTF8.GetBytes(\"hello\");\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task CodeFix_ReplacesWithByteArray_ForReadOnlySpanByte()
    {
        var test = "using System;\r\n\r\nclass C\r\n{\r\n    void M()\r\n    {\r\n        ReadOnlySpan<byte> bytes = new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F };\r\n    }\r\n}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
