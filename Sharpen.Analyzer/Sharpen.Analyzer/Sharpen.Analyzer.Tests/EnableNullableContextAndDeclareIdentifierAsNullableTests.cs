using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp8.EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public sealed class EnableNullableContextAndDeclareIdentifierAsNullableTests
{
    [Fact]
    public async Task Reports_on_reference_type_field_assigned_null_and_reports_on_assignment_location()
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

        // New behavior: diagnostic is reported on the triggering node (assignment), not on the declaration.
        var expected = Verifier.Diagnostic(GeneralRules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
            .WithSpan(8, 9, 8, 18);

        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task Does_not_report_when_identifier_is_already_nullable()
    {
        const string code = @"
class C
{
    private string? _s;

    void M()
    {
        _s = null;
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
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