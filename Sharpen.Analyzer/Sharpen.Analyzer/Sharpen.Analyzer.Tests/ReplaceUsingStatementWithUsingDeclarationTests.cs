using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp8.ReplaceUsingStatementWithUsingDeclarationAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public class ReplaceUsingStatementWithUsingDeclarationTests
{
    [Fact]
    public async Task ReportsDiagnostic_OnOutermostUsingStatement_WhenConvertible_AndNoLeak()
    {
        const string test = @"
using System;

public class Example
{
    public void M()
    {
        using (var d = new Disposable())
        {
            Console.WriteLine(1);
        }
    }

    private sealed class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}";

        var expected = Verifier.Diagnostic()
            .WithSpan(8, 9, 8, 14);

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NoDiagnostic_WhenUsingStatementHasNoDeclaration()
    {
        const string test = @"
using System;

public class Example
{
    public void M(IDisposable d)
    {
        using (d)
        {
            Console.WriteLine(1);
        }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoDiagnostic_WhenUsingIsFollowedByReturnWithExpression()
    {
        const string test = @"
using System;

public class Example
{
    public int M()
    {
        using (var d = new Disposable())
        {
        }

        return 42;
    }

    private sealed class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoDiagnostic_WhenUsingBodyContainsReturnStatement()
    {
        const string test = @"
using System;

public class Example
{
    public int M(bool b)
    {
        using (var d = new Disposable())
        {
            if (b) return 1;
        }

        return 0;
    }

    private sealed class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ReportsDiagnostic_OnOutermostUsing_InNestedChain()
    {
        const string test = @"
using System;

public class Example
{
    public void M()
    {
        using (var d1 = new Disposable())
            using (var d2 = new Disposable())
            {
                Console.WriteLine(1);
            }
    }

    private sealed class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}";

        var expected = new[]
        {
            Verifier.Diagnostic().WithSpan(8, 9, 8, 14),
            Verifier.Diagnostic().WithSpan(8, 9, 8, 14)
        };

        await Verifier.VerifyAnalyzerAsync(test, expected);
    }
}