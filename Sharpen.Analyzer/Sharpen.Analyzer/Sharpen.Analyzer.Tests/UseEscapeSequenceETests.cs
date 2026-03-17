using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseEscapeSequenceEAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseEscapeSequenceETests
{
    [Fact]
    public async Task ReportsDiagnostic_ForStringLiteral_WithU001B()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""\\u001b[31m"";
    }
}";

        await Verifier.VerifyAnalyzerAsync(
            code,
            Verifier.Diagnostic("SHARPEN060")
                .WithSpan(6, 17, 6, 30)
                .WithSeverity(DiagnosticSeverity.Info));
    }
}