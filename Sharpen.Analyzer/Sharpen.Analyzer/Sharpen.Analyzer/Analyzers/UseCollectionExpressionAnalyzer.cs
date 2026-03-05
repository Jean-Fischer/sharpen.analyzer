using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCollectionExpressionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp12Rules.UseCollectionExpressionRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeArrayCreation, SyntaxKind.ArrayCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeImplicitArrayCreation, SyntaxKind.ImplicitArrayCreationExpression);
    }

    private static void AnalyzeArrayCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ArrayCreationExpressionSyntax arrayCreation)
            return;

        // new T[] { ... }
        if (arrayCreation.Initializer == null)
            return;

        // Avoid unsafe/unsupported cases (e.g. stackalloc, omitted sizes, etc.)
        if (arrayCreation.Type == null)
            return;

        // If there is an explicit size, only allow empty size: new T[] { ... }
        // Disallow: new T[3] { ... } (could be rewritten, but keep conservative)
        if (arrayCreation.Type.RankSpecifiers.Count != 1)
            return;

        var rank = arrayCreation.Type.RankSpecifiers[0];
        if (rank.Sizes.Count != 1)
            return;

        if (rank.Sizes[0] is not OmittedArraySizeExpressionSyntax)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp12Rules.UseCollectionExpressionRule, arrayCreation.GetLocation()));
    }

    private static void AnalyzeImplicitArrayCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
            return;

        // new[] { ... }
        if (implicitArrayCreation.Initializer == null)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp12Rules.UseCollectionExpressionRule, implicitArrayCreation.GetLocation()));
    }
}
