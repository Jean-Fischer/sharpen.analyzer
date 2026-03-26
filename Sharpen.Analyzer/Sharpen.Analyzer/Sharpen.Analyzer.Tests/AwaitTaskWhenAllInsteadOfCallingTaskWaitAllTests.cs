using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Tests.Infrastructure;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class AwaitTaskWhenAllInsteadOfCallingTaskWaitAllTests
{
    [Fact]
    public async Task Reports_diagnostic_in_async_method()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        Task.WaitAll(tasks);
    }
}";

        var expected = Verifier.Diagnostic(Rules.Rules.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule)
            .WithSpan(8, 9, 8, 28);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Code_fix_replaces_statement_only_WaitAll_with_await_Task_WhenAll()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        Task.WaitAll(tasks);
    }
}";

        var fixedCode = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        await Task.WhenAll(tasks);
    }
}";

        var expected = Verifier.Diagnostic(Rules.Rules.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule)
            .WithSpan(8, 9, 8, 28);

        await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task Code_fix_is_idempotent()
    {
        var test = new CSharpCodeFixTest<
            Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer,
            Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        Task.WaitAll(tasks);
    }
}",
            FixedCode = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        await Task.WhenAll(tasks);
    }
}",
        };

        test.ExpectedDiagnostics.Add(
            Verifier.Diagnostic(Rules.GeneralRules.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule)
                .WithSpan(8, 9, 8, 28));

        await test.VerifyCodeFixIsIdempotentAsync();
    }
}