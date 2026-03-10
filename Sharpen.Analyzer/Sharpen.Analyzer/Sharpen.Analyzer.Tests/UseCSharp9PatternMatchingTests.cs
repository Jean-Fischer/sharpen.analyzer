using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp9.UseCSharp9PatternMatchingAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp9.UseCSharp9PatternMatchingCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public class UseCSharp9PatternMatchingTests
{
    [Fact]
    public async Task UseCSharp9PatternMatching_TriggersAndFixes_ForNotNullCheck()
    {
        const string original = @"
public class C
{
    public void M(object o)
    {
        if (o != null)
        {
        }
    }
}
";

        const string fixedCode = @"
public class C
{
    public void M(object o)
    {
        if (o is not null)
        {
        }
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 13);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_TriggersAndFixes_ForNegatedIsPattern()
    {
        const string original = @"
public class C
{
    public void M(object o)
    {
        if (!(o is string))
        {
        }
    }
}
";

        const string fixedCode = @"
public class C
{
    public void M(object o)
    {
        if (o is not string)
        {
        }
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 13);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_TriggersAndFixes_ForRangeAnd()
    {
        const string original = @"
public class C
{
    public bool M(int x)
    {
        return x >= 0 && x <= 10;
    }
}
";

        const string fixedCode = @"
public class C
{
    public bool M(int x)
    {
        return x is >= 0 and <= 10;
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 16);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_DoesNotTrigger_ForRangeOrWithNonConstantBounds()
    {
        const string code = @"
public class C
{
    public bool M(int x, int a, int b)
    {
        return x < a || x > b;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_DoesNotTrigger_ForSideEffectExpression()
    {
        const string code = @"
public class C
{
    private object Get() => null;

    public void M()
    {
        if (Get() != null)
        {
        }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_DoesNotTrigger_InsideExpressionTreeLambda()
    {
        const string code = @"
using System;
using System.Linq;
using System.Linq.Expressions;

public class SurveyHistory
{
    public string MarketType { get; set; }
}

public class C
{
    public void M(IQueryable<SurveyHistory> source)
    {
        // This lambda is converted to Expression<Func<...>>.
        // Rewriting `!= null` to `is not null` would fail at runtime/compile-time for expression trees.
        var q = source.Where(s => s.MarketType != null);
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCSharp9PatternMatching_DoesNotTrigger_ForNonIdenticalRepeatedExpression()
    {
        const string code = @"
public class C
{
    public bool M(int x)
    {
        return (x + 1) >= 0 && x <= 10;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    // Note: the test harness compiles with a modern language version, so we can't reliably assert
    // "does not trigger under C# 8" here without custom test plumbing.
}
