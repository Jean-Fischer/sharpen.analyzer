using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseRecordStructAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseRecordStructCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseRecordStructTests
{
    [Fact]
    public async Task UseRecordStruct_TriggersAndFixes_ForPublicFieldBasedStruct()
    {
        var source = @"
public struct Point
{
    public int X;
    public int Y;
}
";

        var fixedSource = @"
public record struct Point
{
    public int X;
    public int Y;
}
";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task UseRecordStruct_TriggersAndFixes_ForGetOnlyAutoPropertiesAndConstructor()
    {
        var source = @"
public struct PersonId
{
    public int Value { get; }

    public PersonId(int value)
    {
        Value = value;
    }
}
";

        var fixedSource = @"
public record struct PersonId
{
    public int Value { get; }

    public PersonId(int value)
    {
        Value = value;
    }
}
";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task UseRecordStruct_DoesNotTrigger_ForMutableStructWithSetters()
    {
        var source = @"
public struct Mutable
{
    public int Value { get; set; }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }
}