using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForGetOnlyPropertiesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForGetOnlyPropertiesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        if (property.ExpressionBody != null)
        {
            return;
        }

        if (property.AccessorList == null)
        {
            return;
        }

        if (property.AccessorList.Accessors.Count != 1)
        {
            return;
        }

        var getter = property.AccessorList.Accessors[0];
        if (!getter.IsKind(SyntaxKind.GetAccessorDeclaration))
        {
            return;
        }

        if (!CSharp6SyntaxHelpers.TryGetSingleReturnExpressionFromGetter(getter, out _))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseExpressionBodyForGetOnlyPropertiesRule, property.Identifier.GetLocation()));
    }
}
