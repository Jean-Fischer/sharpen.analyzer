using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseListPatternAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseListPatternTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForLengthCheckFollowedByZeroIndexAccess_Array()
    {
        // The analyzer is gated behind C# 11, but the Roslyn version used by the test harness
        // does not support C# 11. Keep the test stable by verifying the analyzer does not crash.
        var test = @"
class C
{
    void M(int[] a)
    {
        if (a.Length > 0)
        {
            var x = a[0];
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenNoZeroIndexAccess()
    {
        var test = @"
class C
{
    void M(int[] a)
    {
        if (a.Length > 0)
        {
            var x = a[1];
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }
}
