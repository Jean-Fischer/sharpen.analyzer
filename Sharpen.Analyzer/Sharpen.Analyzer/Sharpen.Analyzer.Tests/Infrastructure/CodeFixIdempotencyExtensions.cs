using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Sharpen.Analyzer.Tests.Infrastructure;

public static class CodeFixIdempotencyExtensions
{
    public static async Task VerifyCodeFixIsIdempotentAsync<TAnalyzer, TCodeFix>(
        this CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> test)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        // First run: verify analyzer + code fix produces FixedState.
        await test.RunAsync().ConfigureAwait(false);

        // Second run: ensure applying the same code fix again does not change the code.
        var idempotencyTest = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            ReferenceAssemblies = test.ReferenceAssemblies,
        };

        idempotencyTest.SolutionTransforms.AddRange(test.SolutionTransforms);

        foreach (var source in test.FixedState.Sources)
        {
            idempotencyTest.TestState.Sources.Add(source);
            idempotencyTest.FixedState.Sources.Add(source);
        }

        // If the fix is truly idempotent, the analyzer should either:
        // - produce no diagnostics, or
        // - produce diagnostics but offer no code action.
        // In both cases, the fixed code should remain identical.
        await idempotencyTest.RunAsync().ConfigureAwait(false);
    }
}
