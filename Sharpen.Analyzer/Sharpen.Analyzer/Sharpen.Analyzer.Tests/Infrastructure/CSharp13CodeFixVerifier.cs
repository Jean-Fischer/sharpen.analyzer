using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Sharpen.Analyzer.Tests.Infrastructure;

/// <summary>
///     Test verifier for C# 13 gated code fixes.
///     This sets:
///     - C# parse options to <see cref="LanguageVersion.Preview" /> (the Roslyn version used by this repo
///     does not expose a dedicated <c>LanguageVersion.CSharp13</c> constant).
///     - A minimal stub for <c>System.Threading.Lock</c> so safety checkers can resolve the symbol.
///     Keep this verifier scoped to tests that need it.
/// </summary>
public static class CSharp13CodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    private const string LockStubSource = @"
namespace System.Threading
{
    public sealed class Lock
    {
        public Lock() { }

        // The C# compiler expects this member for the new lock pattern.
        public Scope EnterScope() => default;

        public readonly ref struct Scope
        {
            public void Dispose() { }
        }
    }
}
";

    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>
            .Diagnostic(diagnosticId);
    }

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>
            .Diagnostic(descriptor);
    }

    public static async Task VerifyAnalyzerAsync(
        string source,
        params DiagnosticResult[] expected)
    {
        var test = CreateTest();
        test.TestCode = source;
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync().ConfigureAwait(false);
    }

    public static async Task VerifyCodeFixAsync(
        string source,
        string fixedSource,
        int? numberOfIncrementalIterations = null,
        int? numberOfFixAllIterations = null)
    {
        var test = CreateTest(numberOfIncrementalIterations, numberOfFixAllIterations);
        AddSources(test, source, fixedSource);
        await test.RunAsync().ConfigureAwait(false);
    }

    public static async Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult expected,
        string fixedSource,
        int? numberOfIncrementalIterations = null,
        int? numberOfFixAllIterations = null)
    {
        var test = CreateTest(numberOfIncrementalIterations, numberOfFixAllIterations);
        AddSources(test, source, fixedSource);
        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync().ConfigureAwait(false);
    }

    private static void AddSources(
        CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> test,
        string source,
        string fixedSource)
    {
        test.TestState.Sources.Add(source);
        test.TestState.Sources.Add(LockStubSource);

        test.FixedState.Sources.Add(fixedSource);
        test.FixedState.Sources.Add(LockStubSource);
    }

    private static CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateTest(
        int? numberOfIncrementalIterations = null,
        int? numberOfFixAllIterations = null)
    {
        var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            // Use a modern target framework so preview C# 13 features that depend on runtime support
            // (e.g. byref-like generics) can compile in fixed-state.
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        if (numberOfIncrementalIterations.HasValue)
            test.NumberOfIncrementalIterations = numberOfIncrementalIterations.Value;

        if (numberOfFixAllIterations.HasValue)
            test.NumberOfFixAllIterations = numberOfFixAllIterations.Value;

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var project = solution.GetProject(projectId)!;
            var parseOptions = (CSharpParseOptions)project.ParseOptions!;

            // Use Preview to exercise the "C# 13 or above" branch.
            project = project.WithParseOptions(parseOptions.WithLanguageVersion(LanguageVersion.Preview));

            return project.Solution;
        });

        return test;
    }
}
