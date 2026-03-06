using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp6.UseNameofExpressionForThrowingArgumentExceptionsAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp6.UseNameofExpressionForThrowingArgumentExceptionsCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseNameofExpressionForThrowingArgumentExceptionsTests
{
    [Fact]
    public async Task ReportsDiagnostic_For_ArgumentNullException_StringLiteral_WhenMatchesParameter()
    {
        const string test = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentNullException(""p"");
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 41, 8, 44);
        await Verifier.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReportsDiagnostic_For_ArgumentException_StringLiteralParamName_WhenMatchesParameter()
    {
        const string test = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentException(""msg"", ""p"");
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 44, 8, 47);
        await Verifier.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenStringLiteralDoesNotMatchAnyParameter()
    {
        const string test = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentNullException(""q"");
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenNameofAlreadyUsed()
    {
        const string test = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentNullException(nameof(p));
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task CodeFix_ReplacesStringLiteralWithNameof()
    {
        const string test = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentNullException(""p"");
    }
}";

        const string fixedCode = @"
using System;

class C
{
    void M(string p)
    {
        throw new ArgumentNullException(nameof(p));
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 41, 8, 44);

        await Verifier.VerifyCodeFixAsync(test, expected, fixedCode).ConfigureAwait(false);
    }
}
