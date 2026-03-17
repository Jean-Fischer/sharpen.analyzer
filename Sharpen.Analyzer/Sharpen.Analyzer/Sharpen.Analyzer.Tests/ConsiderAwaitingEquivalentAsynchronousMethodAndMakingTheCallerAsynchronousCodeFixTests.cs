using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.
    ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousAnalyzer,
    Sharpen.Analyzer.ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixTests
{
    [Fact]
    public async Task
        ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronous_AsyncCaller_ProducesDiagnostic()
    {
        var test = @"
using System.IO;
using System.Threading.Tasks;

class C
{
    async Task M(Stream s)
    {
        [|s.Read(new byte[1], 0, 1)|];
    }
}
";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task
        ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronous_AsyncCaller_CodeFix_RewritesInvocationAndAddsAwait()
    {
        var test = @"
using System.IO;
using System.Threading.Tasks;

class C
{
    async Task M(Stream s)
    {
        [|s.Read(new byte[1], 0, 1)|];
    }
}
";

        var fixedCode = @"
using System.IO;
using System.Threading.Tasks;

class C
{
    async Task M(Stream s)
    {
        await s.ReadAsync(new byte[1], 0, 1);
    }
}
";

        await Verifier.VerifyCodeFixAsync(test, fixedCode);
    }

    [Fact]
    public async Task
        ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronous_NonAsyncCaller_ProducesNoDiagnostic()
    {
        var test = @"
using System.IO;
using System.Threading.Tasks;

class C
{
    Task M(Stream s)
    {
        s.Read(new byte[1], 0, 1);
        return Task.CompletedTask;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(test);
    }
}