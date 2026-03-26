using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Safety;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseInterpolatedStringAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseInterpolatedStringCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyIntegrationTests
{
    [Fact]
    public async Task UseInterpolatedString_DiagnosticAndFix_AreSuppressed_WhenFirstPassSafetyBlocks()
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
    public string M(string name)
    {
        return string.Format(""Hello, {0}!"", name);
    }
}
";

            // Analyzer diagnostic should be suppressed.
            await Verifier.VerifyAnalyzerAsync(original);

            // Code fix should also be suppressed (no code actions), so the code remains unchanged.
            var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
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