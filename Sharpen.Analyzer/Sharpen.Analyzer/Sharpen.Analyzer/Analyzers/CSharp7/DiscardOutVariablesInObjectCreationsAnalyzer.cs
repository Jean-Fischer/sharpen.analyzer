using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiscardOutVariablesInObjectCreationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.DiscardOutVariablesInObjectCreationsRule);

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
        if (argumentList == null) return;

        foreach (var argument in from argument in argumentList.Arguments where argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword) where argument.Expression.IsKind(SyntaxKind.IdentifierName) where OutVariableCandidateHelper.IsCandidate(context.SemanticModel, argument, true) select argument)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.DiscardOutVariablesInObjectCreationsRule,
                argument.RefOrOutKeyword.GetLocation()));
        }
    }
}