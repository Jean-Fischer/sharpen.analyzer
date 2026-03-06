using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForGetOnlyIndexersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForGetOnlyIndexersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
    }

    private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
    {
        var indexer = (IndexerDeclarationSyntax)context.Node;

        if (indexer.ExpressionBody != null)
        {
            return;
        }

        if (indexer.AccessorList == null)
        {
            return;
        }

        if (indexer.AccessorList.Accessors.Count != 1)
        {
            return;
        }

        var getter = indexer.AccessorList.Accessors[0];
        if (!getter.IsKind(SyntaxKind.GetAccessorDeclaration))
        {
            return;
        }

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(getter, out _))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseExpressionBodyForGetOnlyIndexersRule, indexer.ThisKeyword.GetLocation()));
    }
}
