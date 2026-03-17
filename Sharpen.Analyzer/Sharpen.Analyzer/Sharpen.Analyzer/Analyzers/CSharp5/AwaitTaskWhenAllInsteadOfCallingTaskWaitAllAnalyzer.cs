using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsTaskWaitAllInvocation(invocation, context.SemanticModel)) return;

        if (!AsyncModernizationHelpers.CanMakeContainingCallableAsync(invocation, context.SemanticModel)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.Rules.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule,
            invocation.GetLocation()));
    }

    private static bool IsTaskWaitAllInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol is null) return false;

        if (symbol.Name != "WaitAll") return false;

        // Static method on System.Threading.Tasks.Task
        if (symbol.ContainingType?.ToDisplayString() != "System.Threading.Tasks.Task") return false;

        // Only report in already-async callables (matches original smoke suite behavior).
        return AsyncModernizationHelpers.IsWithinAsyncCallable(invocation, semanticModel);
    }
}