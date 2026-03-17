using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

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

        foreach (var argument in from argument in argumentList.Arguments where argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword) where argument.Expression.IsKind(SyntaxKind.IdentifierName) where OutVariableCandidateHelper.IsCandidate(context.SemanticModel, argument, true) select argument)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.DiscardOutVariablesInMethodInvocationsRule,
                argument.RefOrOutKeyword.GetLocation()));
        }
    }
}