using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using Sharpen.Analyzer.Tests.Infrastructure;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FixProviderSafetyCheckerTests
{
    [Fact]
    public async Task CollectionExpressionSafetyChecker_ReturnsSafe_ForCSharpDocument_WithSemanticModel_AndDiagnostic()
    {
        var (_, syntaxTree, semanticModel) =
            await SafetyTestDocumentFactory.CreateAsync("class C { void M() { var x = new int[] { 1, 2, 3 }; } }");
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST0001", "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);

        var checker = new CollectionExpressionSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, diagnostic, CancellationToken.None);

        Assert.True(result.IsSafe);
    }

    [Fact]
    public async Task CollectionExpressionSafetyChecker_ReturnsUnsafe_WhenDiagnosticIsNull()
    {
        var (_, syntaxTree, semanticModel) = await SafetyTestDocumentFactory.CreateAsync("class C { void M() { } }");

        var checker = new CollectionExpressionSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, null!, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("no-diagnostic", result.ReasonId);
    }

    [Fact]
    public async Task StringInterpolationSafetyChecker_ReturnsSafe_ForCSharpDocument_WithSemanticModel_AndDiagnostic()
    {
        var (_, syntaxTree, semanticModel) =
            await SafetyTestDocumentFactory.CreateAsync("class C { void M() { var s = string.Format(\"{0}\", 1); } }");
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST0002", "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);

        var checker = new StringInterpolationSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, diagnostic, CancellationToken.None);

        Assert.True(result.IsSafe);
    }

    [Fact]
    public async Task NullCheckSafetyChecker_ReturnsUnsafe()
    {
        var (_, syntaxTree, semanticModel) = await SafetyTestDocumentFactory.CreateAsync("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST0003", "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);

        var checker = new NullCheckSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("null-check-not-implemented", result.ReasonId);
    }

    [Fact]
    public async Task SwitchExpressionSafetyChecker_ReturnsUnsafe()
    {
        var (_, syntaxTree, semanticModel) = await SafetyTestDocumentFactory.CreateAsync("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST0004", "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);

        var checker = new SwitchExpressionSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("switch-expression-not-implemented", result.ReasonId);
    }

    [Fact]
    public async Task LinqSafetyChecker_ReturnsUnsafe()
    {
        var (_, syntaxTree, semanticModel) = await SafetyTestDocumentFactory.CreateAsync("class C { void M() { } }");
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST0005", "t", "m", "c", DiagnosticSeverity.Info, true),
            Location.None);

        var checker = new LinqSafetyChecker();
        var result = checker.IsSafe(syntaxTree, semanticModel, diagnostic, CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Equal("linq-not-implemented", result.ReasonId);
    }
}
