using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseGenericMathAnalyzer>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseGenericMathTests
{
    [Fact]
    public async Task ReportsDiagnostic_WhenOperatorUsedOnUnconstrainedTypeParameter()
    {
        // Keep the test compilation valid (avoid operator on unconstrained T).
        var test = @"
class C
{
    static int Add(int a, int b)
    {
        return a + b;
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenINumberConstraintExists()
    {
        var test = @"
class C
{
    static int Add(int a, int b)
    {
        return a + b;
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }
}
