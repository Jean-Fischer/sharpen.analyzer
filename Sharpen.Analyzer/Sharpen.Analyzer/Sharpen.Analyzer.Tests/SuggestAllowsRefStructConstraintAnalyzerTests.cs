using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestAllowsRefStructConstraintAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public sealed class SuggestAllowsRefStructConstraintAnalyzerTests
{
    [Fact]
    public async Task Reports_on_generic_method_using_span_of_T()
    {
        var source = @"
using System;

public class C
{
    void M<T>(Span<T> s)
    {
    }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.SuggestAllowsRefStructConstraintRule)
            .WithLocation(6, 10);

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Does_not_report_when_no_generic_parameters()
    {
        var source = @"
using System;

public class C
{
    void M(Span<int> s)
    {
    }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Does_not_report_when_constraints_already_present()
    {
        var source = @"
using System;

public class C
{
    void M<T>(Span<T> s) where T : struct
    {
    }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }
}