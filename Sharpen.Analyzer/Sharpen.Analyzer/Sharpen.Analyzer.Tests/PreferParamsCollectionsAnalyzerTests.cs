using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.PreferParamsCollectionsAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class PreferParamsCollectionsAnalyzerTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForParamsArray()
    {
        var code = @"
class C
{
    void M(params int[] values)
    {
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 12, 4, 31);
        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_ForNonArrayParams()
    {
        var code = @"
class C
{
    void M(params System.ReadOnlySpan<int> values)
    {
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
