using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.UseExpressionBodyForSetAccessorsInIndexersAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp7.UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseExpressionBodyForSetAccessorsInIndexersTests
{
    [Fact]
    public async Task Code_fix_should_convert_to_expression_bodied_set_accessor()
    {
        const string original = @"
class C
{
    private int[] _a = new int[1];

    public int this[int i]
    {
        get => _a[i];
        set { _a[i] = value; }
    }
}";

        const string expected = @"
class C
{
    private int[] _a = new int[1];

    public int this[int i]
    {
        get => _a[i];
        set => _a[i] = value;
    }
}";

        var diagnostic = Verifier.Diagnostic().WithSpan(9, 9, 9, 12);
        await Verifier.VerifyCodeFixAsync(original, diagnostic, expected);
    }
}