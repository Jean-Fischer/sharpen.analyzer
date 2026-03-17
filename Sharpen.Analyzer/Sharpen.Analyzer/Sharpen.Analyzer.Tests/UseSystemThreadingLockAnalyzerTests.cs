using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseSystemThreadingLockAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseSystemThreadingLockAnalyzerTests
{
    [Fact]
    public async Task Reports_for_private_object_used_only_in_lock()
    {
        var code = @"
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

        var expected = Verifier.Diagnostic(CSharp13Rules.UseSystemThreadingLockRule)
            .WithLocation(0);

        await Verifier.VerifyAnalyzerAsync(code, expected);
    }

    [Fact]
    public async Task Does_not_report_when_field_used_outside_lock()
    {
        var code = @"
class C
{
    private readonly object _sync = new();

    void M()
    {
        var x = _sync;
        lock (_sync)
        {
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Does_not_report_for_non_private_field()
    {
        var code = @"
class C
{
    internal readonly object _sync = new();

    void M()
    {
        lock (_sync)
        {
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}