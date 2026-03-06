using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitTaskInsteadOfCallingTaskWaitAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsTaskWaitInvocation(invocation, context.SemanticModel)) return;

        // Only report when we can make the containing callable async.
        if (!AsyncModernizationHelpers.CanMakeContainingCallableAsync(invocation, context.SemanticModel)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.Rules.AwaitTaskInsteadOfCallingTaskWaitRule,
            invocation.GetLocation()));
    }

    private static bool IsTaskWaitInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol is null) return false;

        if (symbol.Name != "Wait") return false;

        // Instance method on System.Threading.Tasks.Task
        if (symbol.ContainingType?.ToDisplayString() != "System.Threading.Tasks.Task") return false;

        // Only report in already-async callables (matches original smoke suite behavior).
        return AsyncModernizationHelpers.IsWithinAsyncCallable(invocation, semanticModel);
    }
}
