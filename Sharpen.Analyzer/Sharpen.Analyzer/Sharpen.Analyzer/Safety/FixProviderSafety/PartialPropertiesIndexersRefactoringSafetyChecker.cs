using System.Linq;
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
        if (!TryGetPropertyOrIndexer(
                syntaxTree,
                diagnostic,
                cancellationToken,
                out var propertyOrIndexer,
                out var failureCode))
        {
            return Unsafe(failureCode);
        }

        var containingType = propertyOrIndexer.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType?.Modifiers.Any(SyntaxKind.PartialKeyword) != true)
            return Unsafe("not-in-partial-type");

        if (containingType is InterfaceDeclarationSyntax)
            return Unsafe("interface-member");

        if (!IsEligibleForPartialMemberRefactoring(propertyOrIndexer))
            return Unsafe(GetIneligibleMemberReason(propertyOrIndexer));

        if (!HasAutoAccessors(propertyOrIndexer))
            return Unsafe("non-auto-accessor");

        if (semanticModel.GetDeclaredSymbol(propertyOrIndexer, cancellationToken) is null)
            return Unsafe("symbol-null");

        if (!propertyOrIndexer.AccessorList!.Accessors.Any())
            return Unsafe("no-accessors");

        if (HasExplicitInterfaceSpecifier(propertyOrIndexer))
            return Unsafe("explicit-interface-impl");

        return FixProviderSafetyResult.Safe();
    }

    private static bool TryGetPropertyOrIndexer(
        SyntaxTree syntaxTree,
        Diagnostic diagnostic,
        CancellationToken cancellationToken,
        out BasePropertyDeclarationSyntax propertyOrIndexer,
        out string failureCode)
    {
        propertyOrIndexer = null!;

        if (diagnostic is null)
        {
            failureCode = "diagnostic-null";
            return false;
        }

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
        {
            failureCode = "not-csharp";
            return false;
        }

        propertyOrIndexer = syntaxTree.GetRoot(cancellationToken)
            .FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<BasePropertyDeclarationSyntax>()!;
        if (propertyOrIndexer is null)
        {
            failureCode = "property-or-indexer-not-found";
            return false;
        }

        failureCode = string.Empty;
        return true;
    }

    private static bool IsEligibleForPartialMemberRefactoring(BasePropertyDeclarationSyntax propertyOrIndexer)
    {
        return !propertyOrIndexer.Modifiers.Any(SyntaxKind.PartialKeyword)
               && !propertyOrIndexer.Modifiers.Any(SyntaxKind.AbstractKeyword)
               && propertyOrIndexer.AccessorList is not null
               && !IsExpressionBodied(propertyOrIndexer);
    }

    private static string GetIneligibleMemberReason(BasePropertyDeclarationSyntax propertyOrIndexer)
    {
        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.PartialKeyword))
            return "already-partial";

        if (propertyOrIndexer.Modifiers.Any(SyntaxKind.AbstractKeyword))
            return "abstract";

        if (IsExpressionBodied(propertyOrIndexer))
            return "expression-bodied";

        return "no-accessor-list";
    }

    private static bool IsExpressionBodied(BasePropertyDeclarationSyntax propertyOrIndexer)
    {
        return propertyOrIndexer switch
        {
            PropertyDeclarationSyntax { ExpressionBody: not null } => true,
            IndexerDeclarationSyntax { ExpressionBody: not null } => true,
            _ => false
        };
    }

    private static bool HasAutoAccessors(BasePropertyDeclarationSyntax propertyOrIndexer)
    {
        return propertyOrIndexer.AccessorList!.Accessors.All(accessor =>
            accessor.Body is null && accessor.ExpressionBody is null);
    }

    private static bool HasExplicitInterfaceSpecifier(BasePropertyDeclarationSyntax propertyOrIndexer)
    {
        return propertyOrIndexer is PropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null }
            or IndexerDeclarationSyntax { ExplicitInterfaceSpecifier: not null };
    }

    private static FixProviderSafetyResult Unsafe(string code)
    {
        return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, code);
    }
}
