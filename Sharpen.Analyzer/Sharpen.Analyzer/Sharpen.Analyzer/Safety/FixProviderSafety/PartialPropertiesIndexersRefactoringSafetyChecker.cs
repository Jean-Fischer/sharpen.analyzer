using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class PartialPropertiesIndexersRefactoringSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (diagnostic is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Analyzer reports on identifier/this keyword; normalize to the property/indexer declaration.
        var propertyOrIndexer = node.FirstAncestorOrSelf<BasePropertyDeclarationSyntax>();
        if (propertyOrIndexer is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "property-or-indexer-not-found");

        // Must be in a partial type.
        var containingType = propertyOrIndexer.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType?.Modifiers.Any(SyntaxKind.PartialKeyword) != true)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-in-partial-type");

        // Must not already be partial.
        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.PartialKeyword))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "already-partial");

        // Must not be abstract.
        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.AbstractKeyword))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "abstract");

        // Must not be expression-bodied.
        if (propertyOrIndexer is PropertyDeclarationSyntax { ExpressionBody: not null })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "expression-bodied");

        if (propertyOrIndexer is IndexerDeclarationSyntax { ExpressionBody: not null })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "expression-bodied");

        if (propertyOrIndexer.AccessorList is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-accessor-list");

        // Conservative: only auto accessors (no bodies, no expression bodies).
        foreach (var accessor in propertyOrIndexer.AccessorList.Accessors)
        {
            if (accessor.Body is not null || accessor.ExpressionBody is not null)
                return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "non-auto-accessor");
        }

        // Ensure symbol exists (avoid broken code).
        var symbol = semanticModel.GetDeclaredSymbol(propertyOrIndexer, cancellationToken);
        if (symbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "symbol-null");

        // Validate that we can produce a valid partial member:
        // - Must have at least one accessor
        // - Must not be an explicit interface implementation (partial members can't be explicit)
        // - Must not be in an interface (partial properties/indexers are for classes/structs/records)
        if (!propertyOrIndexer.AccessorList.Accessors.Any())
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-accessors");

        if (propertyOrIndexer is PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or IndexerDeclarationSyntax { ExplicitInterfaceSpecifier: not null })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "explicit-interface-impl");

        if (containingType is InterfaceDeclarationSyntax)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "interface-member");

        return FixProviderSafetyResult.Safe();
    }
}