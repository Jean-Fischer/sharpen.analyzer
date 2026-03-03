using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseIndexFromTheEndAnalyzer>;

public sealed class UseIndexFromTheEndTests
{
    [Fact]
    public async Task Does_not_report_anything_yet()
    {
        const string code = @"
class C
{
    void M(int[] a)
    {
        var x = a[0];
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
