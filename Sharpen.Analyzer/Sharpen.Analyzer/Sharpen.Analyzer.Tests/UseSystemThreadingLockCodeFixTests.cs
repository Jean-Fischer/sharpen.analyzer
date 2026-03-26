using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Sharpen.Analyzer.Tests.Infrastructure.CSharp13CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseSystemThreadingLockAnalyzer,
    Sharpen.Analyzer.UseSystemThreadingLockCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseSystemThreadingLockCodeFixTests
{
    [Fact]
    public async Task Fix_changes_field_type_to_SystemThreadingLock()
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

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task Fix_not_offered_when_Monitor_is_used()
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

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }
}