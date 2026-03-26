using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp13.UseEscapeSequenceEAnalyzer,
    Sharpen.Analyzer.UseEscapeSequenceECodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseEscapeSequenceECodeFixTests
{
    [Fact]
    public async Task Fixes_U001B_ToE_InStringLiteral()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""\\u001b[31m"";
    }
}";

        const string fixedCode = @"
class C
{
    void M()
    {
        var s = ""\\e[31m"";
    }
}";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }


    [Fact]
    public async Task DoesNotOfferFix_ForAmbiguousXEscape()
    {
        const string code = @"
class C
{
    void M()
    {
        var s = ""\\x1b2"";
    }
}";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }
}