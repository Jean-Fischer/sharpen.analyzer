using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp6.UseExpressionBodyForGetOnlyIndexersAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp6.UseExpressionBodyForGetOnlyIndexersCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseExpressionBodyForGetOnlyIndexersTests
{
    [Fact]
    public async Task Should_report_diagnostic_for_eligible_get_only_indexer()
    {
        const string test = @"
class C
{
    public int this[int i]
    {
        get
        {
            return i;
        }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 16, 4, 20);
        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_expression_bodied_indexer()
    {
        const string test = @"
class C
{
    public int this[int i] => i;
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_indexer_with_setter()
    {
        const string test = @"
class C
{
    public int this[int i]
    {
        get { return i; }
        set { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_getter_with_multiple_statements()
    {
        const string test = @"
class C
{
    public int this[int i]
    {
        get
        {
            var x = i;
            return x;
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Code_fix_should_convert_to_expression_bodied_indexer()
    {
        const string original = @"
class C
{
    public int this[int i]
    {
        get
        {
            return i;
        }
    }
}";

        const string fixedText = @"
class C
{
    public int this[int i]
        => i;
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 16, 4, 20);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }
}