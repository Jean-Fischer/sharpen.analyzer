using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseOutVariablesInMethodInvocationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseOutVariablesInMethodInvocationsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        var argumentList = invocation.ArgumentList;
        if (argumentList == null)
        {
            return;
        }

        foreach (var argument in argumentList.Arguments)
        {
            if (!argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword))
            {
                continue;
            }

            if (!argument.Expression.IsKind(SyntaxKind.IdentifierName))
            {
                continue;
            }

            if (!OutVariableCandidateHelper.IsCandidate(context.SemanticModel, argument, outArgumentCanBeDiscarded: false))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseOutVariablesInMethodInvocationsRule, argument.RefOrOutKeyword.GetLocation()));
        }
    }
}
