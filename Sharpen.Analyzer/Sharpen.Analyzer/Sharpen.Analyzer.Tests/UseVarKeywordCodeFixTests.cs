using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.UseVarKeywordAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp7.UseVarKeywordCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests
{
    public class UseVarKeywordCodeFixTests
    {
        [Fact]
        public async Task UseVarKeywordCodeFix_ReplacesExplicitTypeWithVar()
        {
            const string original = @"
using System.Collections.Generic;

public class Example
{
    public void Test()
    {
        List<string> names = new List<string>();
    }
}
";

            const string fixedText = @"
using System.Collections.Generic;

public class Example
{
    public void Test()
    {
        var names = new List<string>();
    }
}
";

            var expected = Verifier.Diagnostic().WithSpan(8, 9, 8, 21).WithArguments("System.Collections.Generic.List<string>");
            await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
        }
    }
}