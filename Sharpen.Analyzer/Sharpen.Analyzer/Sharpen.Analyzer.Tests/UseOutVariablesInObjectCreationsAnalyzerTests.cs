using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.UseOutVariablesInObjectCreationsAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseOutVariablesInObjectCreationsAnalyzerTests
{
    [Fact]
    public async Task UseOutVariablesInObjectCreationsAnalyzer_TriggersOnOutArgumentWithPriorLocalDeclaration()
    {
        const string text = @"
public class Example
{
    private sealed class C
    {
        public C(out int value)
        {
            value = 42;
        }
    }

    public void Test()
    {
        int x;
        _ = new C(out x);

        // x is used after the out argument, so it is a candidate to become an out variable.
        _ = x;
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(15, 19);

        await Verifier.VerifyAnalyzerAsync(text, expected);
    }

    [Fact]
    public async Task UseOutVariablesInObjectCreationsAnalyzer_DoesNotTriggerWhenOutVariableAlreadyInline()
    {
        const string text = @"
public class Example
{
    private sealed class C
    {
        public C(out int value)
        {
            value = 42;
        }
    }

    public void Test()
    {
        _ = new C(out var x);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }

    [Fact]
    public async Task UseOutVariablesInObjectCreationsAnalyzer_DoesNotTriggerWhenOutArgumentIsAField()
    {
        const string text = @"
public class Example
{
    private sealed class C
    {
        public C(out int value)
        {
            value = 42;
        }
    }

    private int _x;

    public void Test()
    {
        _ = new C(out _x);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(text);
    }
}
