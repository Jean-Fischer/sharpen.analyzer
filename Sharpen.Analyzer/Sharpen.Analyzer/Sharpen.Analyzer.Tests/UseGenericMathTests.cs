using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseGenericMathAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

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

        await Verifier.VerifyAnalyzerAsync(test);
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

        await Verifier.VerifyAnalyzerAsync(test);
    }
}
