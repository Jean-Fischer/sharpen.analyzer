using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Engine.SharpenSuggestions.Common.AsyncAwaitAndAsyncStreams;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class
    ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.Rules.ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        // Reuse the existing engine finder to keep behavior consistent with the already-migrated rule.
        var finder = new HardcodedLookupBasedEquivalentAsynchronousMethodFinder();
        if (!finder.EquivalentAsynchronousCandidateExistsFor(
                invocation,
                semanticModel,
                EquivalentAsynchronousMethodFinder.CallerAsyncStatus.CallerMustBeAsync,
                EquivalentAsynchronousMethodFinder.CallerYieldingStatus.Irrelevant))
            return;

        var diagnostic = Diagnostic.Create(
            Rules.Rules.ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule,
            invocation.GetLocation(),
            invocation.Expression.ToString());

        context.ReportDiagnostic(diagnostic);
    }
}