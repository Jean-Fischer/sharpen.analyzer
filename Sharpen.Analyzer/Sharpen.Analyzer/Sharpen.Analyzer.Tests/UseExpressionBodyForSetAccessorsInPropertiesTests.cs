using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp7.UseExpressionBodyForSetAccessorsInPropertiesAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp7.UseExpressionBodyForSetAccessorsInPropertiesCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseExpressionBodyForSetAccessorsInPropertiesTests
{
    [Fact]
    public async Task Code_fix_should_convert_to_expression_bodied_set_accessor()
    {
        const string original = @"
class C
{
    private int _p;

    public int P
    {
        get => _p;
        set { _p = value; }
    }
}";

        const string expected = @"
class C
{
    private int _p;

    public int P
    {
        get => _p;
        set => _p = value;
    }
}";

        var diagnostic = Verifier.Diagnostic().WithSpan(9, 9, 9, 12);
        await Verifier.VerifyCodeFixAsync(original, diagnostic, expected);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_expression_bodied_set_accessor()
    {
        const string test = @"
class C
{
    private int _p;

    public int P
    {
        get => _p;
        set => _p = value;
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_setter_with_multiple_statements()
    {
        const string test = @"
class C
{
    private int _p;

    public int P
    {
        get => _p;
        set
        {
            _p = value;
            _p++;
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_non_property_set_accessor()
    {
        const string test = @"
class C
{
    private int[] _a = new int[1];

    public int this[int i]
    {
        get => _a[i];
        set { _a[i] = value; }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }
}
