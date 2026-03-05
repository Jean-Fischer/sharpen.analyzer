using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultLambdaParametersAnalyzer,
    Sharpen.Analyzer.FixProvider.UseDefaultLambdaParametersCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseDefaultLambdaParametersTests
{
    [Fact]
    public async Task Fixes_parenthesized_lambda_parameters()
    {
        const string original = @"
using System;

delegate int D(int x = 1, int y = 2);

class C
{
    void M()
    {
        D f = (int x, int y) => x + y;
    }
}
";

        const string fixedCode = @"
using System;

delegate int D(int x = 1, int y = 2);

class C
{
    void M()
    {
        D f = (int x = 1, int y = 2) => x + y;
    }
}
";

        var expected = Verifier.Diagnostic(CSharp12Rules.UseDefaultLambdaParametersRule)
            .WithLocation(10, 15);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task Does_not_trigger_for_implicitly_typed_parameters()
    {
        const string code = @"
using System;

delegate int D(int x = 1);

class C
{
    void M()
    {
        D f = x => x;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task Does_not_trigger_when_delegate_has_no_default()
    {
        const string code = @"
using System;

delegate int D(int x);

class C
{
    void M()
    {
        D f = (int x) => x;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }
}
