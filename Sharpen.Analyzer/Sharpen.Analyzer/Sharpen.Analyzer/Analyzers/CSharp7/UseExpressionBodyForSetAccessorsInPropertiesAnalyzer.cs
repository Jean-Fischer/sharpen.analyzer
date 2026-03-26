using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForSetAccessorsInPropertiesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseExpressionBodyForSetAccessorsInPropertiesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.SetAccessorDeclaration);
    }

    private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
    {
        var accessor = (AccessorDeclarationSyntax)context.Node;

        if (accessor.ExpressionBody != null) return;

        if (accessor.FirstAncestorOrSelf<PropertyDeclarationSyntax>() == null) return;

        if (accessor.Body == null) return;

        if (accessor.Body.Statements.Count != 1) return;

        if (accessor.Body.Statements[0] is not ExpressionStatementSyntax) return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseExpressionBodyForSetAccessorsInPropertiesRule,
            accessor.Keyword.GetLocation()));
    }
}