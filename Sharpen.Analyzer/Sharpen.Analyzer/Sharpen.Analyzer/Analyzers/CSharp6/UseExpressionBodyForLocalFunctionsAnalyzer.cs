using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForLocalFunctionsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseExpressionBodyForLocalFunctionsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        var localFunction = (LocalFunctionStatementSyntax)context.Node;

        if (localFunction.ExpressionBody != null) return;

        if (localFunction.Body == null) return;

        if (localFunction.Body.Statements.Count != 1) return;

        if (!localFunction.Body.Statements[0].IsKind(SyntaxKind.ExpressionStatement)) return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseExpressionBodyForLocalFunctionsRule,
            localFunction.Identifier.GetLocation()));
    }
}