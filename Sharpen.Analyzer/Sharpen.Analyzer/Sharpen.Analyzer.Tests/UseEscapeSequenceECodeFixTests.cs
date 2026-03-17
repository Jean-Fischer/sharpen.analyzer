using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseEscapeSequenceEAnalyzer,
    Sharpen.Analyzer.UseEscapeSequenceECodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseEscapeSequenceECodeFixTests
{
    [Fact]
    public async Task Fixes_U001B_ToE_InStringLiteral()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""\\u001b[31m"";
    }
}";

        const string fixedCode = @"
class C
{
    void M()
    {
        var s = ""\\e[31m"";
    }
}";

        await Verifier.VerifyCodeFixAsync(
            code,
            Verifier.Diagnostic("SHARPEN060")
                .WithSpan(6, 17, 6, 30)
                .WithSeverity(DiagnosticSeverity.Info),
            fixedCode);
    }


    [Fact]
    public async Task DoesNotOfferFix_ForAmbiguousXEscape()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""\\x1b2"";
    }
}";

        await Verifier.VerifyCodeFixAsync(
            code,
            Verifier.Diagnostic("SHARPEN060")
                .WithSpan(6, 17, 6, 25)
                .WithSeverity(DiagnosticSeverity.Info),
            code);
    }
}