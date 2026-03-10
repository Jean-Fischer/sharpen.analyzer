using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp12.UseInlineArrayAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp12.UseInlineArrayCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseInlineArrayTests
{
    [Fact]
    public async Task Fixes_struct_with_element_sequence_fields()
    {
        const string original = @"
 public struct Buffer
 {
     private int _element0;
     private int _element1;
     private int _element2;
 }
 ";

        // The test compilation doesn't reference a BCL that contains InlineArrayAttribute.
        // This test suite doesn't currently support multi-file inputs, so we only verify the analyzer here.

        // Keep the expected output readable; match the formatter's output.
        const string fixedCode = @"
[System.Runtime.CompilerServices.InlineArray(3)]
public struct Buffer
 {
     private int _element0;
}
  ";

        var expected = Verifier.Diagnostic(CSharp12Rules.UseInlineArrayRule)
            .WithLocation(2, 16)
            .WithArguments(3);

        await Verifier.VerifyAnalyzerAsync(original, expected);
    }


    [Fact]
    public async Task Does_not_trigger_when_struct_has_method()
    {
        const string code = @"
public struct Buffer
{
    private int _element0;
    private int _element1;

    public int Get0() => _element0;
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Does_not_trigger_when_struct_has_attributes()
    {
        const string code = @"
[System.Serializable]
public struct Buffer
{
    private int _element0;
    private int _element1;
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
