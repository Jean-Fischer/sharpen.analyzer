using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AwaitEquivalentAsynchronousMethodAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitEquivalentAsynchronousMethodRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;


        // Reuse your existing logic
        var finder = new HardcodedLookupBasedEquivalentAsynchronousMethodFinder();
        if (!finder.EquivalentAsynchronousCandidateExistsFor(
                invocation,
                semanticModel,
                EquivalentAsynchronousMethodFinder.CallerAsyncStatus.CallerMustBeAsync,
                EquivalentAsynchronousMethodFinder.CallerYieldingStatus.Irrelevant)) return;
        var diagnostic = Diagnostic.Create(
            Rules.Rules.AwaitEquivalentAsynchronousMethodRule,
            invocation.GetLocation(),
            invocation.Expression.ToString()
        );
        context.ReportDiagnostic(diagnostic);
    }
}