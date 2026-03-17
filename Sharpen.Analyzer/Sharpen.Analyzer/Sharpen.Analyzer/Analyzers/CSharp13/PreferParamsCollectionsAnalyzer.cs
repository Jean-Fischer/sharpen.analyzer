using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferParamsCollectionsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.PreferParamsCollectionsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax method)
            return;

        AnalyzeParameterList(context, method.ParameterList);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConstructorDeclarationSyntax ctor)
            return;

        AnalyzeParameterList(context, ctor.ParameterList);
    }

    private static void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalFunctionStatementSyntax localFunction)
            return;

        AnalyzeParameterList(context, localFunction.ParameterList);
    }

    private static void AnalyzeParameterList(SyntaxNodeAnalysisContext context, ParameterListSyntax? parameterList)
    {
        if (parameterList is null)
            return;

        foreach (var parameter in parameterList.Parameters)
        {
            if (!parameter.Modifiers.Any(SyntaxKind.ParamsKeyword))
                continue;

            // Only consider params arrays: params T[]
            if (parameter.Type is not ArrayTypeSyntax)
                continue;

            // Keep conservative: only single-dimensional arrays.
            if (parameter.Type is ArrayTypeSyntax arrayType && arrayType.RankSpecifiers.Count != 1)
                continue;

            context.ReportDiagnostic(Diagnostic.Create(
                CSharp13Rules.PreferParamsCollectionsRule,
                parameter.GetLocation()));
        }
    }
}