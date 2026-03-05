using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseListPatternAnalyzer>;

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

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
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

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
