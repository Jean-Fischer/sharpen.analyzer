using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public class AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyTests
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
        Task.WaitAny(tasks);
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule)
            .WithSpan(8, 9, 8, 28);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task Code_fix_replaces_statement_only_WaitAny_with_await_Task_WhenAny()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        Task.WaitAny(tasks);
    }
}";

        var fixedCode = @"
using System.Threading.Tasks;

class C
{
    async Task M(Task[] tasks)
    {
        await Task.WhenAny(tasks);
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule)
            .WithSpan(8, 9, 8, 28);

        await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
    }

    [Fact]
    public async Task Code_fix_is_not_offered_when_return_value_is_used()
    {
        var test = @"
using System.Threading.Tasks;

class C
{
    async Task<int> M(Task[] tasks)
    {
        return Task.WaitAny(tasks);
    }
}";

        var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule)
            .WithSpan(8, 16, 8, 35);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }
}
