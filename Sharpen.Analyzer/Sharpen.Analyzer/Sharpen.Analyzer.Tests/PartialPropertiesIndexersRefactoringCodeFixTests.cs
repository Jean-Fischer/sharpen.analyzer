using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.PartialPropertiesIndexersRefactoringAnalyzer,
    Sharpen.Analyzer.PartialPropertiesIndexersRefactoringCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class PartialPropertiesIndexersRefactoringCodeFixTests
{
    [Fact]
    public async Task When_AutoPropertyInPartialType_Then_FixAddsDeclaringAndImplementingPartialMembers()
    {
        const string code = @"
partial class C
{
    public int {|#0:P|} { get; set; }
}
";

        const string fixedCode = @"
partial class C
{
    public partial int P { get; set; }
    public partial int P { get
        {
            throw new System.NotImplementedException();
        }

        set
        {
            throw new System.NotImplementedException();
        }
    }
}
";

        var expected = Verifier.Diagnostic(CSharp13Rules.PartialPropertiesIndexersRefactoringRule)
            .WithLocation(0);

        await Verifier.VerifyCodeFixAsync(code, expected, fixedCode);
    }

    // Indexers are excluded for now (see analyzer implementation notes).

    [Fact]
    public async Task When_NotInPartialType_Then_NoFix()
    {
        const string code = @"
class C
{
    public int P { get; set; }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}