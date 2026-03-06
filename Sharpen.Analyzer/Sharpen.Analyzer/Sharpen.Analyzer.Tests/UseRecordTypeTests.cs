using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp9.UseRecordTypeAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp9.UseRecordTypeCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public class UseRecordTypeTests
{
    [Fact]
    public async Task UseRecordType_TriggersAndFixes_ForSealedDataClass()
    {
        const string original = @"
public sealed class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
";

        const string fixedCode = @"
public record Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
};
";

        var expected = Verifier.Diagnostic().WithLocation(2, 21);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseRecordType_DoesNotTrigger_WhenHasMethod()
    {
        const string code = @"
public sealed class Point
{
    public int X { get; }

    public Point(int x)
    {
        X = x;
    }

    public int DoubleX() => X * 2;
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseRecordType_DoesNotTrigger_WhenHasField()
    {
        const string code = @"
public sealed class Point
{
    private int _x;

    public int X { get; }

    public Point(int x)
    {
        _x = x;
        X = x;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseRecordType_DoesNotTrigger_WhenHasBaseClass()
    {
        const string code = @"
public class Base { }

public sealed class Point : Base
{
    public int X { get; }

    public Point(int x)
    {
        X = x;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }
}
