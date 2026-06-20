using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp5.AwaitEquivalentAsynchronousMethodAnalyzer,
    Sharpen.Analyzer.AwaitEquivalentAsynchronousMethodCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

public class AwaitEquivalentAsynchronousMethodCodeFixTests
{
    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodAnalyzer_InvocationOutsideMethod_ProducesNoDiagnostic()
    {
        const string test = @"
using System.IO;

public class Example
{
    private readonly string _ = new StringReader(""test"").ReadToEnd();
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ReplacesSynchronousCallWithAsync()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader(""test"");
        reader.ReadToEnd();
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader(""test"");
        await reader.ReadToEndAsync();
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(10, 9, 10, 27).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_AlreadyAwaited_StaysSingleAwait()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task TestAsync()
    {
        var reader = new StringReader(""test"");
        await reader.ReadToEndAsync();
    }
}";

        await Verifier.VerifyAnalyzerAsync(original);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_AssignmentRhs_IsAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = reader.ReadToEnd();
        return s;
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = await reader.ReadToEndAsync();
        return s;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(10, 17, 10, 35).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_AssignmentRhs_WithParentheses_IsAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = """";
        s = (reader.ReadToEnd());
        return s;
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = """";
        s = (await reader.ReadToEndAsync());
        return s;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(11, 14, 11, 32).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_AssignmentRhs_WithNestedParentheses_IsAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = """";
        s = ((reader.ReadToEnd()));
        return s;
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        var s = """";
        s = ((await reader.ReadToEndAsync()));
        return s;
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(11, 15, 11, 33).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ReturnStatement_IsReturnAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        return reader.ReadToEnd();
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        return await reader.ReadToEndAsync();
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(10, 16, 10, 34).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ReturnStatement_WithParentheses_IsReturnAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        return (reader.ReadToEnd());
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        var reader = new StringReader(""test"");
        return (await reader.ReadToEndAsync());
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(10, 17, 10, 35).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_AsyncLocalFunction_IsAwaited()
    {
        const string original = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        async Task<string> LocalAsync()
        {
            var reader = new StringReader(""test"");
            return reader.ReadToEnd();
        }

        return await LocalAsync();
    }
}";

        const string fixedText = @"
using System.IO;
using System.Threading.Tasks;

public class Example
{
    public async Task<string> TestAsync()
    {
        async Task<string> LocalAsync()
        {
            var reader = new StringReader(""test"");
            return await reader.ReadToEndAsync();
        }

        return await LocalAsync();
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(12, 20, 12, 38).WithArguments("reader.ReadToEnd");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodAnalyzer_NonAsyncCaller_ProducesNoDiagnostic()
    {
        const string original = @"
using System.IO;

public class Example
{
    public void Test()
    {
        var reader = new StringReader(""test"");
        reader.ReadToEnd();
    }
}";

        await Verifier.VerifyAnalyzerAsync(original);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ExtensionMethodEquivalent_IsRewritten()
    {
        const string original = @"
using System.Threading.Tasks;

public static class Extensions
{
    public static int M(this int x) => x;
    public static Task<int> MAsync(this int x) => Task.FromResult(x);
}

public class Example
{
    public async Task<int> TestAsync()
    {
        return 1.M();
    }
}";

        const string fixedText = @"
using System.Threading.Tasks;

public static class Extensions
{
    public static int M(this int x) => x;
    public static Task<int> MAsync(this int x) => Task.FromResult(x);
}

public class Example
{
    public async Task<int> TestAsync()
    {
        return await 1.MAsync();
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(14, 16, 14, 21).WithArguments("1.M");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
     }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_OptionalCancellationToken_IsAccepted()
    {
        const string original = @"
using System.Threading;
using System.Threading.Tasks;

public static class Extensions
{
    public static int M(this int x) => x;
    public static Task<int> MAsync(this int x, CancellationToken cancellationToken = default) => Task.FromResult(x);
}

public class Example
{
    public async Task<int> TestAsync()
    {
        return 1.M();
    }
}";

        const string fixedText = @"
using System.Threading;
using System.Threading.Tasks;

public static class Extensions
{
    public static int M(this int x) => x;
    public static Task<int> MAsync(this int x, CancellationToken cancellationToken = default) => Task.FromResult(x);
}

public class Example
{
    public async Task<int> TestAsync()
    {
        return await 1.MAsync();
    }
}";

        var expected = Verifier.Diagnostic().WithSpan(15, 16, 15, 21).WithArguments("1.M");
        await Verifier.VerifyCodeFixAsync(original, expected, fixedText);
    }

    [Fact]
    public async Task AwaitEquivalentAsynchronousMethodAnalyzer_NoAsyncEquivalent_ProducesNoDiagnostic()
     {
         const string original = @"
 using System.Threading.Tasks;
 
 public class Example
 {
     public int M() => 1;
 
     public async Task<int> TestAsync()
     {
         return M();
     }
 }";
 
         await Verifier.VerifyAnalyzerAsync(original);
     }
 
     [Fact]
     public async Task AwaitEquivalentAsynchronousMethodAnalyzer_IgnoredMethod_ProducesNoDiagnostic()
     {
        // We don't reference EF Core in the test project; provide a minimal stub with the same full name.
        const string original = @"
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public class DbSet<T>
    {
        public void Add(T entity) { }
        public void AddRange(params T[] entities) { }
    }
}

public class Example
{
    public async Task TestAsync(Microsoft.EntityFrameworkCore.DbSet<int> set)
    {
        set.Add(1);
    }
}";

        await Verifier.VerifyAnalyzerAsync(original);
    }
}