using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp6.UseExpressionBodyForGetAccessorsInIndexersAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp6.UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public class UseExpressionBodyForGetAccessorsInIndexersTests
{
    [Fact]
    public async Task Should_report_diagnostic_for_eligible_get_accessor_in_indexer()
    {
        const string test = @"
class C
{
    private int _i;

    public int this[int i]
    {
        get { return _i; }
        set { }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 9, 8, 12);
        await Verifier.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_expression_bodied_get_accessor_in_indexer()
    {
        const string test = @"
class C
{
    private int _i;

    public int this[int i]
    {
        get => _i;
        set { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task Should_not_report_diagnostic_for_get_only_indexer()
    {
        const string test = @"
class C
{
    private int _i;

    public int this[int i]
    {
        get { return _i; }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task Code_fix_should_convert_get_accessor_to_expression_body()
    {
        const string original = @"
class C
{
    private int _i;

    public int this[int i]
    {
        get { return _i; }
        set { }
    }
}";

        const string fixedText = @"
class C
{
    private int _i;

    public int this[int i]
    {
        get => _i;
        set { }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 9, 8, 12);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText).ConfigureAwait(false);
    }
}
