using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Tests.Infrastructure;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.SuggestAllowsRefStructConstraintAnalyzer,
    Sharpen.Analyzer.FixProviders.FixProvider.CSharp13.SuggestAllowsRefStructConstraintCodeFixProvider>;

public sealed class SuggestAllowsRefStructConstraintCodeFixProviderTests
{
    [Fact]
    public async System.Threading.Tasks.Task Adds_constraint_to_generic_method()
    {
        var source = @"
public class C
{
    void M<T>(T value)
    {
        M(ref value);
    }

    void M<T>(ref T value)
    {
    }
}
";

        var fixedSource = @"
public class C
{
    void M<T>(T value)
    {
        M(ref value);
    }

    void M<T>(ref T value)
where T : allows ref struct
    {
    }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.SuggestAllowsRefStructConstraintRule)
            .WithLocation(9, 10);

        await Verifier.VerifyCodeFixAsync(source, expected, fixedSource);
    }
}
