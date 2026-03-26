using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp6.UseExpressionBodyForGetOnlyPropertiesAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp6.UseExpressionBodyForGetOnlyPropertiesCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseExpressionBodyForGetOnlyPropertiesTests
{
    [Fact]
    public async Task Should_report_diagnostic_for_eligible_get_only_property()
    {
        const string test = @"
class C
{
    public int P
    {
        get
        {
            return 1;
        }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 16, 4, 17);
        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_expression_bodied_property()
    {
        const string test = @"
class C
{
    public int P => 1;
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_property_with_setter()
    {
        const string test = @"
class C
{
    public int P
    {
        get { return 1; }
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
    public int P
    {
        get
        {
            var x = 1;
            return x;
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Code_fix_should_convert_to_expression_bodied_property()
    {
        const string original = @"
class C
{
    public int P
    {
        get
        {
            return 1;
        }
    }
}";

        const string fixedText = @"
class C
{
    public int P
        => 1;
}";

        var expected = Verifier.Diagnostic().WithSpan(4, 16, 4, 17);
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }
}