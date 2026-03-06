using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.UseVarKeywordAnalyzer>;

namespace Sharpen.Analyzer.Tests
{
    public class UseVarKeywordAnalyzerTests
    {
        [Fact]
        public async Task UseVarKeywordAnalyzer_TriggersOnExplicitType()
        {
            const string text = @"
using System.Collections.Generic;

public class Example
{
    public void Test()
    {
        List<string> names = new List<string>();
    }
}
";

            var expected = Verifier.Diagnostic()
                .WithLocation(8, 9)
                .WithArguments("System.Collections.Generic.List<string>");

            await Verifier.VerifyAnalyzerAsync(text, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task UseVarKeywordAnalyzer_DoesNotTriggerOnVar()
        {
            const string text = @"
using System.Collections.Generic;

public class Example
{
    public void Test()
    {
        var names = new List<string>();
    }
}
";

            await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
        }

        [Fact]
        public async Task UseVarKeywordAnalyzer_DoesNotTriggerOnNonObjectCreation()
        {
            const string text = @"
public class Example
{
    public void Test()
    {
        string greeting = ""Hello"";
    }
}
";

            await Verifier.VerifyAnalyzerAsync(text).ConfigureAwait(false);
        }
    }
}