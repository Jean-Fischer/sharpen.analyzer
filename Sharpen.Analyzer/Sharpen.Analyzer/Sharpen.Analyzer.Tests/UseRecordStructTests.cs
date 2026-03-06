using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseRecordStructAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseRecordStructCodeFixProvider>;

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

        await Verifier.VerifyCodeFixAsync(
            source,
            new[]
            {
                Verifier.Diagnostic("SHARPEN042").WithSpan(2, 15, 2, 20).WithSeverity(DiagnosticSeverity.Info)
            },
            fixedSource);
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

        await Verifier.VerifyCodeFixAsync(
            source,
            new[]
            {
                Verifier.Diagnostic("SHARPEN042").WithSpan(2, 15, 2, 23).WithSeverity(DiagnosticSeverity.Info)
            },
            fixedSource);
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
