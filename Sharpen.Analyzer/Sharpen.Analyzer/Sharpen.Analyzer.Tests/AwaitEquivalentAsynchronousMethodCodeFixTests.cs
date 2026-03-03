using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.AwaitEquivalentAsynchronousMethodAnalyzer,
    Sharpen.Analyzer.AwaitEquivalentAsynchronousMethodCodeFixProvider>;

public class AwaitEquivalentAsynchronousMethodCodeFixTests
{
    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ReplacesSynchronousCallWithAsync()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader(""test"");
        reader.ReadToEnd();
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader(""test"");
        await reader.ReadToEndAsync();
    }
}";

        await Verifier.VerifyCodeFixAsync(original, fixedText);
    }
}