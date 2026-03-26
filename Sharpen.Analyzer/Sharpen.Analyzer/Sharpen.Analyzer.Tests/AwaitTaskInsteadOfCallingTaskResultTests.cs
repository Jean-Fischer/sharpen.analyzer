using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskInsteadOfCallingTaskResultAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskInsteadOfCallingTaskResultCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public class AwaitTaskInsteadOfCallingTaskResultTests
{
    [Fact]
    public async Task ReportsDiagnostic_AndFixes_SimpleAssignment()
    {
        const string original = @"
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync(Task<int> task)
    {
        var result = task.Result;
    }
}";

        const string fixedText = @"
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync(Task<int> task)
    {
        var result = await task;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 27, 8, 33);
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task ReportsDiagnostic_ButNoFix_ForComplexExpression()
    {
        const string original = @"
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync(Task<int> task)
    {
        var result = task.Result + 100;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(8, 27, 8, 33);
        await Verifier.VerifyAnalyzerAsync(original, expected);
    }

    [Fact]
    public async Task NoDiagnostic_InNonAsyncMethod()
    {
        const string original = @"
using System.Threading.Tasks;

public class Example
{
    public void Test(Task<int> task)
    {
        var result = task.Result;
    }
}";

        await Verifier.VerifyAnalyzerAsync(original);
    }

    [Fact]
    public async Task DiagnosticButNoFix_WhenAwaitNotLegal_LockStatement()
    {
        const string original = @"
using System.Threading.Tasks;

public class Example
{
    private readonly object _gate = new object();

    public async Task TestAsync(Task<int> task)
    {
        lock (_gate)
        {
            var result = task.Result;
        }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(12, 31, 12, 37);
        await Verifier.VerifyAnalyzerAsync(original, expected);
    }
}