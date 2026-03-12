using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PartialPropertiesIndexersRefactoringAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.PartialPropertiesIndexersRefactoringRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeIndexer, SyntaxKind.IndexerDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        if (!IsCandidate(property, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.PartialPropertiesIndexersRefactoringRule,
            property.Identifier.GetLocation()));
    }

    private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
    {
        var indexer = (IndexerDeclarationSyntax)context.Node;

        if (!IsCandidate(indexer, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.PartialPropertiesIndexersRefactoringRule,
            indexer.ThisKeyword.GetLocation()));
    }

    private static bool IsCandidate(BasePropertyDeclarationSyntax propertyOrIndexer, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
    {
        // Conservative first iteration:
        // - Must be inside a partial type
        // - Must be an auto-property/indexer (no accessor bodies)
        // - Must not already be partial
        // - Must not be expression-bodied
        // - Must not be abstract
        //
        // This is enough to support a safe refactoring that splits into:
        //   partial <type> P { get; set; }
        //   partial <type> P { get => field; set => field = value; }
        // (or get-only / init-only variants)

        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return false;
        }

        if (propertyOrIndexer is PropertyDeclarationSyntax { ExpressionBody: not null })
        {
            return false;
        }

        if (propertyOrIndexer is IndexerDeclarationSyntax { ExpressionBody: not null })
        {
            return false;
        }

        if (propertyOrIndexer.AccessorList is null)
        {
            return false;
        }

        // Only auto accessors (no bodies, no expression bodies)
        foreach (var accessor in propertyOrIndexer.AccessorList.Accessors)
        {
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
            {
                return false;
            }
        }

        // Must be in a partial type
        var containingType = propertyOrIndexer.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is null || !containingType.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return false;
        }

        // Ensure symbol exists (avoid broken code)
        var symbol = semanticModel.GetDeclaredSymbol(propertyOrIndexer, cancellationToken);
        return symbol is not null;
    }
}
