using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.CSharp14.SuggestCompoundAssignmentOperatorsAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class SuggestCompoundAssignmentOperatorsTests
{
    [Fact]
    public async Task ReportDiagnostic_WhenTypeDefinesBinaryPlusOperator_AndUsesPlusEquals()
    {
        var code = @"using System;

public struct Counter
{
    public int Value;

    public static Counter operator +(Counter left, Counter right) => new Counter { Value = left.Value + right.Value };
}

public static class Test
{
    public static void M()
    {
        var c = new Counter();
        c {|#0:+=|} new Counter();
    }
}";

        await Verifier.VerifyAnalyzerAsync(
            code,
            new DiagnosticResult(CSharp14Rules.SuggestCompoundAssignmentOperatorsRule)
                .WithLocation(0)
                .WithArguments("Counter"));
    }

    // NOTE: We intentionally do not test the "no diagnostic when operator += exists" case here,
    // because the current repo toolchain does not compile the C# 14 `operator +=` syntax.
}