using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseLambdaParameterModifiersWithoutTypesAnalyzer,
    Sharpen.Analyzer.UseLambdaParameterModifiersWithoutTypesCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseLambdaParameterModifiersWithoutTypesTests
{
    [Fact]
    public async Task ReportsDiagnostic_AndFixes_WhenTargetDelegateTypeIsKnown()
    {
        const string code = @"
using System;

delegate int RefIntToInt(ref int x);

class C
{
    void M()
    {
        RefIntToInt f = (ref int x) => x;
    }
}";

        const string fixedCode = @"
using System;

delegate int RefIntToInt(ref int x);

class C
{
    void M()
    {
        RefIntToInt f = (ref x) => x;
    }
}";

        var expected = Verifier.Diagnostic("SHARPEN068")
            .WithSpan(10, 25, 10, 36)
            .WithSeverity(DiagnosticSeverity.Info);

        await Verifier.VerifyCodeFixAsync(code, expected, fixedCode);
    }

    [Fact]
    public async Task NoDiagnostic_WhenNotTargetTyped()
    {
        const string code = @"
using System;

delegate int RefIntToInt(ref int x);

class C
{
    void M()
    {
        var f = (ref int x) => x;
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}