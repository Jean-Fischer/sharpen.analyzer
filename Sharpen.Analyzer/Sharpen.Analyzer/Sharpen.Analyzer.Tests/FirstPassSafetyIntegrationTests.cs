using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Safety;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp12.UseCollectionExpressionAnalyzer,
    Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class FirstPassSafetyIntegrationTests
{
    [Fact]
    public async Task UseCollectionExpression_NoFix_WhenFirstPassSafetyBlocks()
    {
        FirstPassSafety.Gate = new FirstPassSafetyGate(new IFirstPassSafetyCheck[]
        {
            new AlwaysUnsafeCheck()
        });

        try
        {
            const string original = @"
public class C
{
    public int[] M()
    {
        return new[] { 1, 2, 3 };
    }
}
";

            // If the safety gate blocks, the analyzer should not report the diagnostic.
            await Verifier.VerifyAnalyzerAsync(original);
        }
        finally
        {
            FirstPassSafety.Gate = FirstPassSafetyGate.Empty;
            FirstPassSafety.UnsafeLogger = null;
        }
    }

    [Fact]
    public async Task UnsafeOutcome_IsObservable_ViaLogger()
    {
        SafetyResult? observed = null;

        FirstPassSafety.Gate = new FirstPassSafetyGate(new IFirstPassSafetyCheck[]
        {
            new AlwaysUnsafeCheck()
        });
        FirstPassSafety.UnsafeLogger = r => observed = r;

        try
        {
            const string original = @"
public class C
{
    public int[] M()
    {
        return new[] { 1, 2, 3 };
    }
}
";

            // Trigger code fix discovery (this is where the safety gate runs).
            // The analyzer diagnostic is suppressed, so we don't expect any diagnostics.
            await Verifier.VerifyCodeFixAsync(original, original);

            Assert.NotNull(observed);
            Assert.False(observed!.Value.IsSafe);
            Assert.Equal("test.always-unsafe", observed.Value.ReasonId);
        }
        finally
        {
            FirstPassSafety.Gate = FirstPassSafetyGate.Empty;
            FirstPassSafety.UnsafeLogger = null;
        }
    }

    private sealed class AlwaysUnsafeCheck : IFirstPassSafetyCheck
    {
        public SafetyResult IsSafe(
            SyntaxTree? syntaxTree,
            SemanticModel semanticModel,
            Diagnostic? diagnostic,
            CancellationToken cancellationToken = default)
        {
            return SafetyResult.Unsafe("test.always-unsafe");
        }
    }
}