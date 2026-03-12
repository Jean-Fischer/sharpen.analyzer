using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Sharpen.Analyzer.Tests.Infrastructure;

/// <summary>
/// Test verifier for C# 13 gated code fixes.
///
/// This sets:
/// - C# parse options to <see cref="LanguageVersion.Preview"/> (the Roslyn version used by this repo
///   does not expose a dedicated <c>LanguageVersion.CSharp13</c> constant).
/// - A minimal stub for <c>System.Threading.Lock</c> so safety checkers can resolve the symbol.
///
/// Keep this verifier scoped to tests that need it.
/// </summary>
public static class CSharp13CodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor);

    public static async System.Threading.Tasks.Task VerifyAnalyzerAsync(
        string source,
        params DiagnosticResult[] expected)
    {
        var test = CreateTest();
        test.TestCode = source;
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync().ConfigureAwait(false);
    }

    public static async System.Threading.Tasks.Task VerifyCodeFixAsync(
        string source,
        string fixedSource)
    {
        var test = CreateTest();
        test.TestState.Sources.Add(source);
        test.TestState.Sources.Add(LockStubSource);

        test.FixedState.Sources.Add(fixedSource);
        test.FixedState.Sources.Add(LockStubSource);

        await test.RunAsync().ConfigureAwait(false);
    }

    public static async System.Threading.Tasks.Task VerifyCodeFixAsync(
        string source,
        DiagnosticResult expected,
        string fixedSource)
    {
        var test = CreateTest();
        test.TestState.Sources.Add(source);
        test.TestState.Sources.Add(LockStubSource);

        test.FixedState.Sources.Add(fixedSource);
        test.FixedState.Sources.Add(LockStubSource);

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync().ConfigureAwait(false);
    }

    private static Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier> CreateTest()
    {
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Default,
        };

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
}
