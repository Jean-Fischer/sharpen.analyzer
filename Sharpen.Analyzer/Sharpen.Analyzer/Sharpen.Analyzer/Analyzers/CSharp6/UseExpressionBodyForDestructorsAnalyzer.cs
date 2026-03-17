using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExpressionBodyForDestructorsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseExpressionBodyForDestructorsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeDestructor, SyntaxKind.DestructorDeclaration);
    }

    private static void AnalyzeDestructor(SyntaxNodeAnalysisContext context)
    {
        var dtor = (DestructorDeclarationSyntax)context.Node;

        if (dtor.ExpressionBody != null) return;

        if (dtor.Body == null) return;

        if (dtor.Body.Statements.Count != 1) return;

        if (!dtor.Body.Statements[0].IsKind(SyntaxKind.ExpressionStatement)) return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseExpressionBodyForDestructorsRule,
            dtor.TildeToken.GetLocation()));
    }
}