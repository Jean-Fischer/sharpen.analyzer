using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForSetAccessorsInIndexersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForSetAccessorsInIndexersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.SetAccessorDeclaration);
    }

    private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
    {
        var accessor = (AccessorDeclarationSyntax)context.Node;

        if (accessor.ExpressionBody != null)
        {
            return;
        }

        if (accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>() == null)
        {
            return;
        }

        if (accessor.Body == null)
        {
            return;
        }

        if (accessor.Body.Statements.Count != 1)
        {
            return;
        }

        if (accessor.Body.Statements[0] is not ExpressionStatementSyntax)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseExpressionBodyForSetAccessorsInIndexersRule, accessor.Keyword.GetLocation()));
    }
}
