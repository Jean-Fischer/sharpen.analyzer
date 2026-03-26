using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseImplicitSpanConversionsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseImplicitSpanConversionsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp14OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        // Match: <expr>.AsSpan()
        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name.Identifier.ValueText: "AsSpan"
            } memberAccess)
        {
            return;
        }

        if (invocation.ArgumentList.Arguments.Any())
            return;

        // Only consider when the invocation is used as an argument in another invocation.
        // This keeps the rule conservative and aligns with the spec scenarios.
        if (invocation.Parent is not ArgumentSyntax argument)
            return;

        if (argument.Parent is not ArgumentListSyntax argumentList)
            return;

        if (argumentList.Parent is not InvocationExpressionSyntax outerInvocation)
            return;

        // Ensure this is the BCL AsSpan extension/instance method.
        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol symbol)
            return;

        if (symbol.Name != "AsSpan")
            return;

        if (symbol.ContainingType?.ToDisplayString() != "System.MemoryExtensions")
            return;

        // Ensure removing AsSpan does not change overload resolution.
        if (context.SemanticModel.GetSymbolInfo(outerInvocation, context.CancellationToken).Symbol is not IMethodSymbol beforeSymbol)
            return;

        var receiverExpression = memberAccess.Expression;
        var newArgument = argument.WithExpression(receiverExpression.WithTriviaFrom(argument.Expression));
        var newArgumentList = argumentList.ReplaceNode(argument, newArgument);
        var newOuterInvocation = outerInvocation.WithArgumentList(newArgumentList);

        if (context.SemanticModel.GetSpeculativeSymbolInfo(
                outerInvocation.SpanStart,
                newOuterInvocation,
                SpeculativeBindingOption.BindAsExpression).Symbol is not IMethodSymbol speculativeSymbol)
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(beforeSymbol, speculativeSymbol))
            return;

        // Report on the invocation expression (the AsSpan call).
        context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseImplicitSpanConversionsRule,
            invocation.GetLocation()));
    }
}