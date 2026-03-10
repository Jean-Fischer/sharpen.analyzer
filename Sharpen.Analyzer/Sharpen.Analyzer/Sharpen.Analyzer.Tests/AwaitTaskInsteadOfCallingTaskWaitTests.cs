using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskInsteadOfCallingTaskWaitAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskInsteadOfCallingTaskWaitCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class AwaitTaskInsteadOfCallingTaskWaitTests
{
    [Fact]
    public async Task Reports_diagnostic_in_async_method()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task task)
    {
        task.Wait();
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule)
            .WithSpan(8, 9, 8, 20);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Does_not_report_diagnostic_in_non_async_method()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    Task M(Task task)
    {
        task.Wait();
        return Task.CompletedTask;
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task Code_fix_replaces_parameterless_Wait_with_await_in_async_method()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task task)
    {
        task.Wait();
    }
}";

        var fixedCode = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task task)
    {
        await task;
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule)
            .WithSpan(8, 9, 8, 20);

        await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task Code_fix_is_not_offered_for_Wait_with_timeout()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task task)
    {
        task.Wait(0);
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule)
            .WithSpan(8, 9, 8, 21);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }
}
