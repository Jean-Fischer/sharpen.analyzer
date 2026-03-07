using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardOutVariablesInMethodInvocationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.DiscardOutVariablesInMethodInvocationsRule);

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

            if (!OutVariableCandidateHelper.IsCandidate(context.SemanticModel, argument, outArgumentCanBeDiscarded: true))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.DiscardOutVariablesInMethodInvocationsRule, argument.RefOrOutKeyword.GetLocation()));
        }
    }
}
