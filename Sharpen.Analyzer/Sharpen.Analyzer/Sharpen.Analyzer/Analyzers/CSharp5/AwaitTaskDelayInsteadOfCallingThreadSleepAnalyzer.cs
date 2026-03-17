using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp5;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitTaskDelayInsteadOfCallingThreadSleepAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskDelayInsteadOfCallingThreadSleepRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!IsThreadSleepInvocation(invocation, context.SemanticModel)) return;

        // Only report when we can make the containing callable async.
        if (!AsyncModernizationHelpers.CanMakeContainingCallableAsync(invocation, context.SemanticModel)) return;

        var diagnostic = Diagnostic.Create(
            Rules.Rules.AwaitTaskDelayInsteadOfCallingThreadSleepRule,
            invocation.GetLocation());

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsThreadSleepInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return false;
        if (memberAccess.Name.Identifier.ValueText != "Sleep") return false;

        var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (symbol == null) return false;

        return symbol.ContainingType?.ToDisplayString() == "System.Threading.Thread";
    }
}