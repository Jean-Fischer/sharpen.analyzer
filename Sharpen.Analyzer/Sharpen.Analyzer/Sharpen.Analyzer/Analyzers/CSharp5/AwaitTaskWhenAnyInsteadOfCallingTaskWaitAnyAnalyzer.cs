using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsTaskWaitAnyInvocation(invocation, context.SemanticModel)) return;

        if (!AsyncModernizationHelpers.CanMakeContainingCallableAsync(invocation, context.SemanticModel)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule,
            invocation.GetLocation()));
    }

    private static bool IsTaskWaitAnyInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol) return false;

        if (symbol.Name != "WaitAny") return false;

        // Static method on System.Threading.Tasks.Task
        return symbol.ContainingType?.ToDisplayString() == "System.Threading.Tasks.Task" &&
               // Only report in already-async callables (matches original smoke suite behavior).
               AsyncModernizationHelpers.IsWithinAsyncCallable(invocation, semanticModel);
    }
}