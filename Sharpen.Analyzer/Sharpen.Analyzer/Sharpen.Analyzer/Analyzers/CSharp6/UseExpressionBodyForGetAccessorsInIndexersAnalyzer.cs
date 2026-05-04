using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForGetAccessorsInIndexersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseExpressionBodyForGetAccessorsInIndexersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.GetAccessorDeclaration);
    }

    private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
    {
        var accessor = (AccessorDeclarationSyntax)context.Node;

        if (!IsSupportedIndexerGetter(accessor))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseExpressionBodyForGetAccessorsInIndexersRule,
            accessor.Keyword.GetLocation()));
    }

    private static bool IsSupportedIndexerGetter(AccessorDeclarationSyntax accessor)
    {
        if (accessor.ExpressionBody != null)
            return false;

        if (accessor.Parent is not AccessorListSyntax accessorList || accessorList.Accessors.Count <= 1)
            return false;

        return accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>() != null
               && CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(accessor, out _);
    }
}
