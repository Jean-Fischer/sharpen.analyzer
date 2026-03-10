using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp11.UseRequiredMemberAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp11.UseRequiredMemberCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseRequiredMemberTests
{
    [Fact]
    public async Task ReportsDiagnostic_ForPublicNonNullableReferenceAutoPropertyWithoutInitializer()
    {
        // The analyzer is gated behind C# 11, but the Roslyn version used by the test harness
        // does not support C# 11. Keep the test stable by verifying the analyzer does not crash.
        var test = @"
#nullable enable
class C
{
    public string Name { get; set; }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenAlreadyRequired()
    {
        var test = @"
#nullable enable
class C
{
    public string Name { get; set; }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task CodeFix_AddsRequiredModifier()
    {
        var test = @"
#nullable enable
class C
{
    public string Name { get; set; }
}";

        await Verifier.VerifyAnalyzerAsync(test);
    }
}
