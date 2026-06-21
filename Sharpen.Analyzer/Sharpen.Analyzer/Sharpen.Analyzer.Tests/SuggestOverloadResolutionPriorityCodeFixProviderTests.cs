using System.Threading.Tasks;
using Sharpen.Analyzer.Analyzers.CSharp13;
using Sharpen.Analyzer.FixProvider.CSharp13;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestOverloadResolutionPriorityAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp13.SuggestOverloadResolutionPriorityCodeFixProvider>;

public sealed class SuggestOverloadResolutionPriorityCodeFixProviderTests
{
    [Fact]
    public async Task Adds_attribute_to_method()
    {
        const string source = @"
public class C
{
    public void M(int x) { }
    public void M(params object[] args) { }
}
";

        const string fixedSource = @"
public class C
{
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public void M(int x) { }
    public void M(params object[] args) { }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.SuggestOverloadResolutionPriorityRule)
            .WithLocation(4, 17);

        var test = Verifier.CreateTest(numberOfFixAllIterations: 1);

        test.TestState.Sources.Add(source);
        test.FixedState.Sources.Add(fixedSource);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }
}
