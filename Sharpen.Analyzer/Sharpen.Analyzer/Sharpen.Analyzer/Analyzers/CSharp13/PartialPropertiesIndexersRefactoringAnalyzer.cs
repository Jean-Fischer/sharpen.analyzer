using System.Collections.Immutable;
using System.Threading;
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

        if (!IsCandidate(property, context.SemanticModel, context.CancellationToken)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.PartialPropertiesIndexersRefactoringRule,
            property.Identifier.GetLocation()));
    }

    private static void AnalyzeIndexer(SyntaxNodeAnalysisContext context)
    {
        var indexer = (IndexerDeclarationSyntax)context.Node;

        if (!IsCandidate(indexer, context.SemanticModel, context.CancellationToken)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.PartialPropertiesIndexersRefactoringRule,
            indexer.ThisKeyword.GetLocation()));
    }

    private static bool IsCandidate(BasePropertyDeclarationSyntax propertyOrIndexer, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Conservative first iteration:
        // - Must be inside a partial type (class/struct/record)
        // - Must be an auto-property (indexers are excluded for now)
        // - Must not already be partial
        // - Must not be expression-bodied
        // - Must not be abstract
        // - Must not be explicit interface implementation
        //
        // Note: C# 13 partial indexers exist, but Roslyn's current preview support in this repo
        // does not allow auto-indexers without bodies, which makes it hard to test safely.

        if (propertyOrIndexer is not PropertyDeclarationSyntax property) return false;

        if (property.Modifiers.Any(SyntaxKind.PartialKeyword)) return false;

        if (property.Modifiers.Any(SyntaxKind.AbstractKeyword)) return false;

        if (property.ExpressionBody is not null) return false;

        if (property.ExplicitInterfaceSpecifier is not null) return false;

        if (property.AccessorList is null) return false;

        // Only auto accessors (no bodies, no expression bodies)
        foreach (var accessor in property.AccessorList.Accessors)
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
                return false;

        // Must be in a partial type
        var containingType = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is null || !containingType.Modifiers.Any(SyntaxKind.PartialKeyword)) return false;

        if (containingType is InterfaceDeclarationSyntax) return false;

        // Ensure symbol exists (avoid broken code)
        var symbol = semanticModel.GetDeclaredSymbol(property, cancellationToken);
        return symbol is not null;
    }
}