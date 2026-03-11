using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.Analyzers.CSharp12;

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

        if (!IsSafeToReport(context, arrayCreation))
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

        if (!IsSafeToReport(context, implicitArrayCreation))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp12Rules.UseCollectionExpressionRule, implicitArrayCreation.GetLocation()));
    }

    private static bool IsSafeToReport(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        // Analyzer-side safety gate: only report diagnostics when the global safety gate allows it.
        // This keeps analyzer diagnostics aligned with fix-provider behavior.
        var evaluation = FixProviderSafetyRunner.Evaluate(
            semanticModel: context.SemanticModel,
            fixProviderType: typeof(UseCollectionExpressionCodeFixProvider),
            node: expression,
            diagnostic: null,
            cancellationToken: context.CancellationToken);

        return evaluation.Outcome == FixProviderSafetyOutcome.Safe;
    }
}
