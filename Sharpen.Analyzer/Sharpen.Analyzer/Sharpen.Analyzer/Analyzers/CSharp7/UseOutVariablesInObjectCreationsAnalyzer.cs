using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseOutVariablesInObjectCreationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseOutVariablesInObjectCreationsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var argumentList = objectCreation.ArgumentList;
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

            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseOutVariablesInObjectCreationsRule, argument.RefOrOutKeyword.GetLocation()));
        }
    }
}
