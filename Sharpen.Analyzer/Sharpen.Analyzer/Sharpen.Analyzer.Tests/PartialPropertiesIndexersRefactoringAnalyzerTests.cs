using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.PartialPropertiesIndexersRefactoringAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class PartialPropertiesIndexersRefactoringAnalyzerTests
{
    [Fact]
    public async Task When_AutoPropertyInPartialType_Then_DiagnosticIsReported()
    {
        const string code = @"
partial class C
{
    public int {|#0:P|} { get; set; }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.PartialPropertiesIndexersRefactoringRule)
            .WithLocation(0);

        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task When_AutoIndexerInPartialType_Then_NoDiagnostic()
    {
        // Indexers are excluded for now (see analyzer implementation notes).
        const string code = @"
partial class C
{
    public int this[int i]
    {
        get { return 0; }
        set { }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_NotInPartialType_Then_NoDiagnostic()
    {
        const string code = @"
class C
{
    public int P { get; set; }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_NonAutoAccessor_Then_NoDiagnostic()
    {
        const string code = @"
partial class C
{
    public int P
    {
        get { return 1; }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_ExpressionBodiedProperty_Then_NoDiagnostic()
    {
        const string code = @"
partial class C
{
    public int P => 1;
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_AbstractProperty_Then_NoDiagnostic()
    {
        const string code = @"
abstract partial class C
{
    public abstract int P { get; set; }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_AlreadyPartial_Then_NoDiagnostic()
    {
        // Provide both parts so the code compiles.
        const string code = @"
partial class C
{
    public partial int P { get; set; }

    public partial int P
    {
        get { return 0; }
        set { }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_ExplicitInterfaceImplementation_Then_NoDiagnostic()
    {
        const string code = @"
interface I
{
    int P { get; }
}

partial class C : I
{
    int I.P { get; }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task When_InInterface_Then_NoDiagnostic()
    {
        const string code = @"
partial interface I
{
    int P { get; }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}