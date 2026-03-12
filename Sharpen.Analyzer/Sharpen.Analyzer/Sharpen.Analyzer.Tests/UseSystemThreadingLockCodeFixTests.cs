using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Tests.Infrastructure;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseSystemThreadingLockAnalyzer,
    Sharpen.Analyzer.UseSystemThreadingLockCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseSystemThreadingLockCodeFixTests
{
    [Fact]
    public async System.Threading.Tasks.Task Fix_changes_field_type_to_SystemThreadingLock()
    {
        var before = @"
class C
{
    private readonly object {|#0:_sync|} = new();

    void M()
    {
        lock (_sync)
        {
        }
    }
}";

        var after = @"
class C
{
    private readonly System.Threading.Lock _sync = new();

    void M()
    {
        lock (_sync)
        {
        }
    }
}";

        var expected = Verifier.Diagnostic(CSharp13Rules.UseSystemThreadingLockRule)
            .WithLocation(0);

        await Verifier.VerifyCodeFixAsync(before, expected, after);
    }

    [Fact]
    public async System.Threading.Tasks.Task Fix_not_offered_when_Monitor_is_used()
    {
        var code = @"
using System.Threading;

class C
{
    private readonly object _sync = new();

    void M()
    {
        Monitor.Enter(_sync);
        try
        {
            lock (_sync)
            {
            }
        }
        finally
        {
            Monitor.Exit(_sync);
        }
    }
}";

        await Verifier.VerifyCodeFixAsync(code, code);
    }
}
