using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp3;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseDefaultExpressionInOptionalConstructorParametersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseDefaultExpressionInOptionalConstructorParametersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
    {
        var ctor = (ConstructorDeclarationSyntax)context.Node;

        foreach (var parameter in ctor.ParameterList.Parameters)
        {
            if (parameter.Default?.Value is not DefaultExpressionSyntax defaultExpression) continue;

            var defaultExpressionType = context.SemanticModel.GetTypeInfo(defaultExpression).Type;
            var parameterType = context.SemanticModel.GetTypeInfo(parameter.Type!).Type;

            if (defaultExpressionType is null || parameterType is null) continue;

            if (!SymbolEqualityComparer.Default.Equals(defaultExpressionType, parameterType)) continue;

            context.ReportDiagnostic(Diagnostic.Create(
                Rules.GeneralRules.UseDefaultExpressionInOptionalConstructorParametersRule,
                defaultExpression.Keyword.GetLocation(),
                parameterType.ToDisplayString()));
        }
    }
}