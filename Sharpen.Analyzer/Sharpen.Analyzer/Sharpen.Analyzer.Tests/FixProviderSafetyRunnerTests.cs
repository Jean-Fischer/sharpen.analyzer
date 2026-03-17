using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Sharpen.Analyzer.Safety;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyRunnerTests
{
    [Fact]
    public void EvaluateOrMatchFailed_ReturnsMatchFailed_WhenMatchSucceededIsFalse()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = CreateDiagnostic("TEST0001");

        var evaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new AlwaysSafeChecker(),
            document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!,
            semanticModel,
            diagnostic,
            false,
            CancellationToken.None);

        Assert.Equal(FixProviderSafetyOutcome.MatchFailed, evaluation.Outcome);
        Assert.Null(evaluation.SafetyResult);
    }

    [Fact]
    public void EvaluateOrMatchFailed_ReturnsUnsafeGlobal_AndDoesNotEvaluateLocal_WhenGlobalIsUnsafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = CreateDiagnostic("TEST0002");

        var localChecker = new CountingChecker(true);

        var originalGate = FirstPassSafety.Gate;
        try
        {
            // Force global stage to be unsafe.
            FirstPassSafety.Gate = new FirstPassSafetyGate(new IFirstPassSafetyCheck[]
            {
                new AlwaysUnsafeFirstPassCheck()
            });

            var evaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
                localChecker,
                document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!,
                semanticModel,
                diagnostic,
                true,
                CancellationToken.None);

            Assert.Equal(FixProviderSafetyOutcome.Unsafe, evaluation.Outcome);
            Assert.NotNull(evaluation.SafetyResult);
            Assert.Equal(FixProviderSafetyStage.Global, evaluation.SafetyResult!.Value.Stage);
            Assert.Equal(0, localChecker.CallCount);
        }
        finally
        {
            FirstPassSafety.Gate = originalGate;
        }
    }

    [Fact]
    public void EvaluateOrMatchFailed_ReturnsUnsafeLocal_WhenGlobalIsSafe_AndLocalIsUnsafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = CreateDiagnostic("TEST0003");

        var originalGate = FirstPassSafety.Gate;
        try
        {
            // Force global stage to be safe.
            FirstPassSafety.Gate = FirstPassSafetyGate.Empty;

            var evaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
                new AlwaysUnsafeChecker(),
                document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!,
                semanticModel,
                diagnostic,
                true,
                CancellationToken.None);

            Assert.Equal(FixProviderSafetyOutcome.Unsafe, evaluation.Outcome);
            Assert.NotNull(evaluation.SafetyResult);
            Assert.Equal(FixProviderSafetyStage.Local, evaluation.SafetyResult!.Value.Stage);
        }
        finally
        {
            FirstPassSafety.Gate = originalGate;
        }
    }

    [Fact]
    public void EvaluateOrMatchFailed_ReturnsSafe_WhenGlobalIsSafe_AndLocalIsSafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = CreateDiagnostic("TEST0004");

        var originalGate = FirstPassSafety.Gate;
        try
        {
            // Force global stage to be safe.
            FirstPassSafety.Gate = FirstPassSafetyGate.Empty;

            var evaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
                new AlwaysSafeChecker(),
                document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!,
                semanticModel,
                diagnostic,
                true,
                CancellationToken.None);

            Assert.Equal(FixProviderSafetyOutcome.Safe, evaluation.Outcome);
            Assert.Null(evaluation.SafetyResult);
        }
        finally
        {
            FirstPassSafety.Gate = originalGate;
        }
    }

    private static Diagnostic CreateDiagnostic(string id)
    {
        return Diagnostic.Create(
            new DiagnosticDescriptor(id, "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);
    }

    private static (Document document, SemanticModel semanticModel) CreateCSharpDocumentAndSemanticModel(string source)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var document = workspace.AddDocument(project.Id, "Test0.cs", SourceText.From(source));
        var semanticModel = document.GetSemanticModelAsync(CancellationToken.None).GetAwaiter().GetResult();

        Assert.NotNull(semanticModel);
        return (document, semanticModel!);
    }

    private sealed class AlwaysSafeChecker : IFixProviderSafetyChecker
    {
        public FixProviderSafetyResult IsSafe(SyntaxTree syntaxTree, SemanticModel semanticModel, Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            return FixProviderSafetyResult.Safe();
        }
    }

    private sealed class AlwaysUnsafeChecker : IFixProviderSafetyChecker
    {
        public FixProviderSafetyResult IsSafe(SyntaxTree syntaxTree, SemanticModel semanticModel, Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "local-unsafe");
        }
    }

    private sealed class CountingChecker : IFixProviderSafetyChecker
    {
        private readonly bool _isSafe;

        public CountingChecker(bool isSafe)
        {
            _isSafe = isSafe;
        }

        public int CallCount { get; private set; }

        public FixProviderSafetyResult IsSafe(SyntaxTree syntaxTree, SemanticModel semanticModel, Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return _isSafe
                ? FixProviderSafetyResult.Safe()
                : FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "local-unsafe");
        }
    }

    private sealed class AlwaysUnsafeFirstPassCheck : IFirstPassSafetyCheck
    {
        public SafetyResult IsSafe(SyntaxTree? syntaxTree, SemanticModel semanticModel, Diagnostic? diagnostic,
            CancellationToken cancellationToken = default)
        {
            return SafetyResult.Unsafe("global-unsafe", "forced");
        }
    }
}