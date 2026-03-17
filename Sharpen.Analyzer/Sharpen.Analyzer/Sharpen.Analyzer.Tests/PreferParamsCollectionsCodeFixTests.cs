using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.PreferParamsCollectionsAnalyzer,
    Sharpen.Analyzer.PreferParamsCollectionsCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class PreferParamsCollectionsCodeFixTests
{
    [Fact]
    public async Task Fix_ChangesSignature_AndWrapsExpandedArgs()
    {
        var before = @"
class C
{
    void M(params int[] values)
    {
    }

    void Call()
    {
        M(1, 2, 3);
    }
}";

        var after = @"
class C
{
    void M(params global::System.ReadOnlySpan<int> values)
    {
    }

    void Call()
    {
        M(1, 2, 3);
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 12, 4, 31);
        await Verifier.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async Task Fix_IsBlocked_WhenArraySemanticsUsed()
    {
        var code = @"
class C
{
    void M(params int[] values)
    {
        var x = values.Length;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 12, 4, 31);
        await Verifier.VerifyCodeFixAsync(code, expected, code);
    }
}