using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp9.UseTopLevelStatementsAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp9.UseTopLevelStatementsCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseTopLevelStatementsTests
{
    [Fact]
    public async Task UseTopLevelStatements_Triggers_ForSimpleMain()
    {
        var code = @"
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(""Hello"");
    }
}
";

        // Note: the Roslyn test harness compiles snippets as a library by default.
        // Top-level statements require an executable output kind, so we only verify the analyzer here.
        var expected = Verifier.Diagnostic().WithLocation(4, 7);
        await Verifier.VerifyAnalyzerAsync(code, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseTopLevelStatements_DoesNotTrigger_WhenNamespacePresent()
    {
        var code = @"
namespace N
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseTopLevelStatements_DoesNotTrigger_WhenAdditionalTypePresent()
    {
        var code = @"
class Program
{
    static void Main(string[] args)
    {
    }
}

class Other { }
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseTopLevelStatements_DoesNotTrigger_WhenTypeofProgramUsed()
    {
        var code = @"
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(typeof(Program));
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseTopLevelStatements_DoesNotTrigger_WhenDirectivesPresent()
    {
        var code = @"
#define X

class Program
{
    static void Main(string[] args)
    {
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }
}
