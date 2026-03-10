using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp8.EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public sealed class EnableNullableContextAndDeclareIdentifierAsNullableTests
{
    [Fact]
    public async Task Reports_on_reference_type_field_assigned_null()
    {
        const string code = @"
class C
{
    private string _s;

    void M()
    {
        _s = null;
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
            .WithSpan(4, 20, 4, 22);

        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task Does_not_report_on_value_type_field_assigned_null()
    {
        const string code = @"
class C
{
    private int _i;

    void M()
    {
        _i = 0;
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
