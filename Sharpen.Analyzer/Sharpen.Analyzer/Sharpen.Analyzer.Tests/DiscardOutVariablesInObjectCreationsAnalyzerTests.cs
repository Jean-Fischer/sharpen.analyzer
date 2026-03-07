using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.DiscardOutVariablesInObjectCreationsAnalyzer>;

namespace Sharpen.Analyzer.Tests;

public sealed class DiscardOutVariablesInObjectCreationsAnalyzerTests
{
    [Fact]
    public async Task DiscardOutVariablesInObjectCreationsAnalyzer_TriggersOnUnusedOutArgument()
    {
        const string text = @"
class C
{
    private sealed class D
    {
        public D(out int value)
        {
            value = 123;
        }
    }

    void M()
    {
        int value;
        _ = new D(out value);
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(15, 19);

        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task DiscardOutVariablesInObjectCreationsAnalyzer_DoesNotTriggerWhenOutArgumentIsUsedAfterObjectCreation()
    {
        const string text = @"
class C
{
    private sealed class D
    {
        public D(out int value)
        {
            value = 123;
        }
    }

    void M()
    {
        int value;
        _ = new D(out value);
        _ = value;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }

    [Fact]
    public async Task DiscardOutVariablesInObjectCreationsAnalyzer_DoesNotTriggerWhenOutArgumentIsAField()
    {
        const string text = @"
class C
{
    private sealed class D
    {
        public D(out int value)
        {
            value = 123;
        }
    }

    private int _value;

    void M()
    {
        _ = new D(out _value);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }
}
