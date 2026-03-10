using System.Threading.Tasks;
using Xunit;
using Verifier =
    Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<Sharpen.Analyzer.SampleSyntaxAnalyzer,
        Sharpen.Analyzer.SampleCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class SampleCodeFixProviderTests
{
    [Fact]
    public async Task ClassWithMyCompanyTitle_ReplaceWithCommonKeyword()
    {
        const string text = @"
public class MyCompanyClass
{
}
";

        const string newText = @"
public class CommonClass
{
}
";

        var expected = Verifier.Diagnostic()
            .WithLocation(2, 14)
            .WithArguments("MyCompanyClass");
        await Verifier.VerifyCodeFixAsync(text, expected, newText);
    }
}