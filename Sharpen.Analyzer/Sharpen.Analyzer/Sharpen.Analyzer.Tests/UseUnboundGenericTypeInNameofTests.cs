using System.Threading.Tasks;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseUnboundGenericTypeInNameofAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp14.UseUnboundGenericTypeInNameofCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseUnboundGenericTypeInNameofTests
{
    [Fact]
    public async Task ReportsDiagnostic_AndFixes_NameofOnClosedGeneric()
    {
        const string code = @"
using System.Collections.Generic;

class C
{
    string M() => nameof([|Dictionary<string, int>|]);
}";

        const string fixedCode = @"
using System.Collections.Generic;

class C
{
    string M() => nameof(Dictionary<,>);
}";

        await Verifier.VerifyCodeFixAsync(
            code,
            fixedCode,
            numberOfIncrementalIterations: 1,
            numberOfFixAllIterations: 1);
    }

    [Fact]
    public async Task NoDiagnostic_ForNonGenericNameof()
    {
        const string code = @"
class C
{
    string M() => nameof(C);
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
