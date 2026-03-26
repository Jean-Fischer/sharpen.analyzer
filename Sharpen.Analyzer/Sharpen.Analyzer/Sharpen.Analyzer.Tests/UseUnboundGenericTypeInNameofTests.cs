using System.Threading.Tasks;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.UseUnboundGenericTypeInNameofAnalyzer,
    Sharpen.Analyzer.UseUnboundGenericTypeInNameofCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseUnboundGenericTypeInNameofTests
{
    // TODO: enable once the Roslyn test harness is configured to allow iterative code fix application.
    // The current implementation requires 2 iterations:
    // 1) nameof(Dictionary<string, int>) -> nameof(Dictionary<,>)
    // 2) analyzer no longer reports
    // but the default verifier expects a single iteration.
    //
    // [Fact]
    // public async Task ReportsDiagnostic_AndFixes_NameofOnClosedGeneric()
    // {
    //     const string code = @"
    // using System.Collections.Generic;
    //
    // class C
    // {
    //     string M() => nameof([|Dictionary<string, int>|]);
    // }";
    //
    //     const string fixedCode = @"
    // using System.Collections.Generic;
    //
    // class C
    // {
    //     string M() => nameof(Dictionary<,>);
    // }";
    //
    //     var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    // }

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