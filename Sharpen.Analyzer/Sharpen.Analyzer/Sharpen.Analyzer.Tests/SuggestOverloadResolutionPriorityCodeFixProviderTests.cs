using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Analyzers.CSharp13;
using Sharpen.Analyzer.FixProviders.FixProvider.CSharp13;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestOverloadResolutionPriorityAnalyzer,
    Sharpen.Analyzer.FixProviders.FixProvider.CSharp13.SuggestOverloadResolutionPriorityCodeFixProvider>;

public sealed class SuggestOverloadResolutionPriorityCodeFixProviderTests
{
    [Fact]
    public async Task Adds_attribute_to_method()
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

        var test = new CSharpCodeFixTest<
            SuggestOverloadResolutionPriorityAnalyzer,
            SuggestOverloadResolutionPriorityCodeFixProvider,
            DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90,
            NumberOfFixAllIterations = 1
        };

        test.TestState.Sources.Add(source);
        test.FixedState.Sources.Add(fixedSource);
        test.ExpectedDiagnostics.Add(expected);

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            var parseOptions = (CSharpParseOptions)project.ParseOptions!;
            project = project.WithParseOptions(parseOptions.WithLanguageVersion(LanguageVersion.Preview));
            return project.Solution;
        });

        await test.RunAsync().ConfigureAwait(false);
    }
}