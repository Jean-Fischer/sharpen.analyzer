using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestOverloadResolutionPriorityAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public sealed class SuggestOverloadResolutionPriorityAnalyzerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Reports_on_overload_set_with_params_object_array()
    {
        var source = @"
public class C
{
    public void M(int x) { }
    public void M(params object[] args) { }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.SuggestOverloadResolutionPriorityRule)
            .WithLocation(4, 17);

        await Verifier.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async System.Threading.Tasks.Task Does_not_report_when_no_overloads()
    {
        var source = @"
public class C
{
    public void M(int x) { }
    public void N(params object[] args) { }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }
}
