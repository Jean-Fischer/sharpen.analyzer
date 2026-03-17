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
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForGetAccessorsInIndexersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAccessor, SyntaxKind.GetAccessorDeclaration);
    }

    private static void AnalyzeAccessor(SyntaxNodeAnalysisContext context)
    {
        var accessor = (AccessorDeclarationSyntax)context.Node;

        if (accessor.ExpressionBody != null) return;

        if (accessor.Parent is not AccessorListSyntax accessorList) return;

        // Must have a set accessor as well (otherwise C# 6 get-only indexer rule applies).
        if (accessorList.Accessors.Count <= 1) return;

        if (accessor.FirstAncestorOrSelf<IndexerDeclarationSyntax>() == null) return;

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(accessor, out _)) return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseExpressionBodyForGetAccessorsInIndexersRule,
            accessor.Keyword.GetLocation()));
    }
}