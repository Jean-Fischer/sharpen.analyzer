using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UsePrimaryConstructorAnalyzer>;

namespace Sharpen.Analyzer.Tests;

public sealed class UsePrimaryConstructorTests
{
    [Fact]
    public async Task Reports_diagnostic_for_simple_class_with_auto_properties()
    {
        const string original = @"
public class C
{
    public int X { get; set; }
    public string Y { get; set; }

    public C(int x, string y)
    {
        X = x;
        Y = y;
    }
}
";

        var expected = Verifier.Diagnostic(CSharp12Rules.UsePrimaryConstructorRule)
            .WithLocation(7, 12);

        await Verifier.VerifyAnalyzerAsync(original, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task Does_not_trigger_when_constructor_has_extra_statement()
    {
        const string code = @"
public class C
{
    public int X { get; set; }

    public C(int x)
    {
        X = x;
        System.Console.WriteLine(x);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task Does_not_trigger_when_multiple_constructors_exist()
    {
        const string code = @"
public class C
{
    public int X { get; set; }

    public C(int x)
    {
        X = x;
    }

    public C()
    {
        X = 0;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task Does_not_trigger_when_base_initializer_present()
    {
        const string code = @"
public class B
{
    public B(int x) { }
}

public class C : B
{
    public int X { get; set; }

    public C(int x) : base(x)
    {
        X = x;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }
}
