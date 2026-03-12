using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UsePartialEventsAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UsePartialEventsTests
{
    [Fact]
    public async Task ReportsDiagnostic_WhenEventAccessorsCallPartialMethods()
    {
        var code = @"
partial class C
{
    partial void AddHandler(System.Action value);
    partial void RemoveHandler(System.Action value);

    public event System.Action E
    {
        add { AddHandler(value); }
        remove { RemoveHandler(value); }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(
            code,
            Verifier.Diagnostic("SHARPEN072")
                .WithSpan(7, 32, 7, 33));
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenAccessorCallsNonPartialMethod()
    {
        var code = @"
class C
{
    void AddHandler(System.Action value) { }
    void RemoveHandler(System.Action value) { }

    public event System.Action E
    {
        add { AddHandler(value); }
        remove { RemoveHandler(value); }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
