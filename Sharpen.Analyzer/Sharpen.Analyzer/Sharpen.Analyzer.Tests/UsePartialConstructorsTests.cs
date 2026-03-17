using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UsePartialConstructorsAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UsePartialConstructorsTests
{
    [Fact]
    public async Task ReportsDiagnostic_WhenConstructorCallsPartialInitializationMethod()
    {
        var code = @"
partial class C
{
    partial void InitializeGenerated();

    public C()
    {
        InitializeGenerated();
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            code,
            Verifier.Diagnostic("SHARPEN071")
                .WithSpan(6, 12, 6, 13));
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenConstructorCallsNonPartialMethod()
    {
        var code = @"
class C
{
    void InitializeGenerated() { }

    public C()
    {
        InitializeGenerated();
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}