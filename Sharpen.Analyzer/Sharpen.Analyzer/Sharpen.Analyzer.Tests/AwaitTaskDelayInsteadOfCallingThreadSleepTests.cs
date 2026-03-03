using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.AwaitTaskDelayInsteadOfCallingThreadSleepAnalyzer,
    Sharpen.Analyzer.FixProvider.AwaitTaskDelayInsteadOfCallingThreadSleepCodeFixProvider>;

public class AwaitTaskDelayInsteadOfCallingThreadSleepTests
{
    [Fact]
    public async Task ReportsDiagnostic_AndFixes_InAsyncMethod()
    {
        const string original = @"
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        Thread.Sleep(1000);
    }
}";

        const string fixedText = @"
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        await Task.Delay(1000);
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(9, 9, 9, 27);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task NoDiagnostic_InNonAsyncMethod()
    {
        const string original = @"
using System.Threading;

public class Example
{
    public void Test()
    {
        Thread.Sleep(1000);
    }
}";

        await Verifier.VerifyAnalyzerAsync(original);
    }

    [Fact]
    public async Task DiagnosticButNoFix_WhenAwaitNotLegal_LockStatement()
    {
        const string original = @"
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    private readonly object _gate = new object();

    public async Task TestAsync()
    {
        lock (_gate)
        {
            Thread.Sleep(1000);
        }
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(13, 13, 13, 31);
        await Verifier.VerifyAnalyzerAsync(original, expected);
    }

    [Fact]
    public async Task Fix_PreservesArgumentTrivia()
    {
        const string original = @"
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync(int delay)
    {
        Thread.Sleep(/*ms*/ delay);
    }
}";

        const string fixedText = @"
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync(int delay)
    {
        await Task.Delay(/*ms*/ delay);
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(9, 9, 9, 35);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }
}
