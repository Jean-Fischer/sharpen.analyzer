using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Tests.Infrastructure;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestOverloadResolutionPriorityAnalyzer,
    Sharpen.Analyzer.FixProviders.FixProvider.CSharp13.SuggestOverloadResolutionPriorityCodeFixProvider>;

public sealed class SuggestOverloadResolutionPriorityCodeFixProviderTests
{
    [Fact]
    public async System.Threading.Tasks.Task Adds_attribute_to_method()
    {
        var source = @"
public class C
{
    public void M(int x) { }
    public void M(params object[] args) { }
}
";

        var fixedSource = @"
public class C
{
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public void M(int x) { }
    public void M(params object[] args) { }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.SuggestOverloadResolutionPriorityRule)
            .WithLocation(4, 17);

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
            Sharpen.Analyzer.Analyzers.CSharp13.SuggestOverloadResolutionPriorityAnalyzer,
            Sharpen.Analyzer.FixProviders.FixProvider.CSharp13.SuggestOverloadResolutionPriorityCodeFixProvider,
            Microsoft.CodeAnalysis.Testing.DefaultVerifier>
        {
            ReferenceAssemblies = Microsoft.CodeAnalysis.Testing.ReferenceAssemblies.Net.Net90,
            NumberOfFixAllIterations = 1,
        };

        test.TestState.Sources.Add(source);
        test.FixedState.Sources.Add(fixedSource);
        test.ExpectedDiagnostics.Add(expected);

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            var parseOptions = (Microsoft.CodeAnalysis.CSharp.CSharpParseOptions)project.ParseOptions!;
            project = project.WithParseOptions(parseOptions.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview));
            return project.Solution;
        });

        await test.RunAsync().ConfigureAwait(false);
    }
}
