using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.DiscardOutVariablesInMethodInvocationsAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class DiscardOutVariablesInMethodInvocationsAnalyzerTests
{
    [Fact]
    public async Task DiscardOutVariablesInMethodInvocationsAnalyzer_TriggersOnUnusedOutArgument()
    {
        const string text = @"
class C
{
    void M()
    {
        int value;
        int.TryParse(""123"", out value);
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(7, 29);

        await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task DiscardOutVariablesInMethodInvocationsAnalyzer_DoesNotTriggerWhenOutArgumentIsUsedAfterInvocation()
    {
        const string text = @"
class C
{
    void M()
    {
        int value;
        int.TryParse(""123"", out value);
        _ = value;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }

    [Fact]
    public async Task DiscardOutVariablesInMethodInvocationsAnalyzer_DoesNotTriggerWhenOutArgumentIsAField()
    {
        const string text = @"
class C
{
    private int _value;

    void M()
    {
        int.TryParse(""123"", out _value);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }

    [Fact]
    public async Task DiscardOutVariablesInMethodInvocationsAnalyzer_DoesNotTriggerWhenOutArgumentIsAnArrayElement()
    {
        const string text = @"
class C
{
    void M()
    {
        var values = new int[1];
        int.TryParse(""123"", out values[0]);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
    }
}
