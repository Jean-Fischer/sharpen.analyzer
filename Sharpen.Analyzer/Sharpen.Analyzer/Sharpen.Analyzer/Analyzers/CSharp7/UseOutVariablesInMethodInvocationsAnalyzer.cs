using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseOutVariablesInMethodInvocationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseOutVariablesInMethodInvocationsRule);

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

        foreach (var argument in from argument in argumentList.Arguments where argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword) where argument.Expression.IsKind(SyntaxKind.IdentifierName) where OutVariableCandidateHelper.IsCandidate(context.SemanticModel, argument, false) select argument)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseOutVariablesInMethodInvocationsRule,
                argument.RefOrOutKeyword.GetLocation()));
        }
    }
}