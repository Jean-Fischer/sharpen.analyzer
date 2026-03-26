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
using System.Threading.Tasks;

public class Example
{
    private readonly int _ = Task.CompletedTask.GetAwaiter().GetResult();
}";

        var expectedCompilerError = DiagnosticResult.CompilerError("CS0029")
            .WithSpan(6, 30, 6, 73)
            .WithArguments("void", "int");

        await Verifier.VerifyAnalyzerAsync(test, expectedCompilerError);
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
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
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
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
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
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
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
    public async Task AwaitEquivalentAsynchronousMethodCodeFix_ExtensionMethodEquivalent_IsResolved()
    {
        // NOTE: The analyzer intentionally excludes extension methods (see spec/design).
        // This test is kept as a regression guard to ensure we don't accidentally start reporting on them.
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

        await Verifier.VerifyAnalyzerAsync(original);
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