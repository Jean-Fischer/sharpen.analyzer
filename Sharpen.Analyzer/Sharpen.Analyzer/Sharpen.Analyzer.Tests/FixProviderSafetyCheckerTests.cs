using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyCheckerTests
{
    [Fact]
    public void CollectionExpressionSafetyChecker_ReturnsSafe_ForCSharpDocument_WithSemanticModel_AndDiagnostic()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { var x = new int[] { 1, 2, 3 }; } }");
        var diagnostic = Diagnostic.Create(
            descriptor: new DiagnosticDescriptor("TEST0001", "t", "m", "c", DiagnosticSeverity.Info, isEnabledByDefault: true),
            location: Location.None);

        var checker = new CollectionExpressionSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic, CancellationToken.None);

        Assert.True(result.IsSafe);
    }

    [Fact]
    public void CollectionExpressionSafetyChecker_ReturnsUnsafe_WhenDiagnosticIsNull()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");

        var checker = new CollectionExpressionSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic: null!, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("no-diagnostic", result.ReasonId);
    }

    [Fact]
    public void StringInterpolationSafetyChecker_ReturnsSafe_ForCSharpDocument_WithSemanticModel_AndDiagnostic()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { var s = string.Format(\"{0}\", 1); } }");
        var diagnostic = Diagnostic.Create(
            descriptor: new DiagnosticDescriptor("TEST0002", "t", "m", "c", DiagnosticSeverity.Info, isEnabledByDefault: true),
            location: Location.None);

        var checker = new StringInterpolationSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic, CancellationToken.None);

        Assert.True(result.IsSafe);
    }

    [Fact]
    public void NullCheckSafetyChecker_ReturnsUnsafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            descriptor: new DiagnosticDescriptor("TEST0003", "t", "m", "c", DiagnosticSeverity.Info, isEnabledByDefault: true),
            location: Location.None);

        var checker = new NullCheckSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("null-check-not-implemented", result.ReasonId);
    }

    [Fact]
    public void SwitchExpressionSafetyChecker_ReturnsUnsafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            descriptor: new DiagnosticDescriptor("TEST0004", "t", "m", "c", DiagnosticSeverity.Info, isEnabledByDefault: true),
            location: Location.None);

        var checker = new SwitchExpressionSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("switch-expression-not-implemented", result.ReasonId);
    }

    [Fact]
    public void LinqSafetyChecker_ReturnsUnsafe()
    {
        var (document, semanticModel) = CreateCSharpDocumentAndSemanticModel("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            descriptor: new DiagnosticDescriptor("TEST0005", "t", "m", "c", DiagnosticSeverity.Info, isEnabledByDefault: true),
            location: Location.None);

        var checker = new LinqSafetyChecker();
        var result = checker.IsSafe(document.GetSyntaxTreeAsync(CancellationToken.None).GetAwaiter().GetResult()!, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("linq-not-implemented", result.ReasonId);
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
}
