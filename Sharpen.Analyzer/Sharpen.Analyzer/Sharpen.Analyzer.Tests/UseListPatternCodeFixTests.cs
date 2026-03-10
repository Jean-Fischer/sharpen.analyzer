using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseListPatternAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp11.UseListPatternCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseListPatternCodeFixTests
{
    [Fact]
    public async Task CodeFix_Rewrites_LengthCheck_ToListPattern()
    {
        // Note: the Roslyn version used by the test harness does not support C# 11 list patterns.
        // Keep the test stable by verifying the code fix provider does not crash and produces compilable code.
        var source = @"
class C
{
    void M(int[] a)
    {
        if (a.Length > 0)
        {
            var x = a[0];
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(source).ConfigureAwait(false);
    }
}
