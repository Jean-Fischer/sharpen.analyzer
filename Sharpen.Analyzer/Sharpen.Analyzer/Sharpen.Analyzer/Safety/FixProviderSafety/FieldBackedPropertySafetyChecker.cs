using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Helpers.CSharp14;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class FieldBackedPropertySafetyChecker : IFixProviderSafetyChecker
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

        // Diagnostic is reported on the property identifier.
        var property = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
        if (property is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "property-not-found");

        if (property.AccessorList is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-accessor-list");

        var getAccessor =
            property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        var setAccessor =
            property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (getAccessor is null || setAccessor is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "missing-get-or-set");

        if (!FieldBackedPropertyHelper.TryGetBackingFieldFromGetter(semanticModel, getAccessor, cancellationToken,
                out var backingFieldSymbol) || backingFieldSymbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "backing-field-not-found");

        if (backingFieldSymbol.DeclaredAccessibility != Accessibility.Private)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "field-not-private");

        if (!IsSimpleSetterAssigningField(semanticModel, setAccessor, backingFieldSymbol, cancellationToken))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "setter-not-simple");

        // Reject if the field name is used in nameof(...) anywhere in the containing type.
        // (Conservative: we don't try to prove it's unrelated.)
        if (ContainsNameofFieldIdentifier(property, backingFieldSymbol.Name))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "nameof-usage");

        // Reject if the field identifier is used in any attribute argument in the containing type.
        if (ContainsAttributeFieldIdentifier(property, backingFieldSymbol.Name))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "attribute-usage");

        // Ensure the field is referenced only within the property accessors.
        // This is a best-effort check within the current syntax tree; cross-file/partial checks are not possible here.
        if (!IsFieldReferencedOnlyWithinPropertyAccessors(root, semanticModel, property, backingFieldSymbol,
                cancellationToken))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "field-referenced-elsewhere");

        return FixProviderSafetyResult.Safe();
    }

    private static bool IsSimpleSetterAssigningField(
        SemanticModel semanticModel,
        AccessorDeclarationSyntax setAccessor,
        IFieldSymbol backingFieldSymbol,
        CancellationToken ct)
    {
        ExpressionSyntax? assignedExpression = null;

        if (setAccessor.ExpressionBody is not null)
        {
            assignedExpression = setAccessor.ExpressionBody.Expression;
        }
        else if (setAccessor.Body is not null)
        {
            var statements = setAccessor.Body.Statements;
            if (statements.Count != 1)
                return false;

            if (statements[0] is not ExpressionStatementSyntax expressionStatement)
                return false;

            assignedExpression = expressionStatement.Expression;
        }

        if (assignedExpression is null)
            return false;

        if (assignedExpression is not AssignmentExpressionSyntax assignment ||
            !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return false;

        var leftSymbol = semanticModel.GetSymbolInfo(assignment.Left, ct).Symbol;
        if (!SymbolEqualityComparer.Default.Equals(leftSymbol, backingFieldSymbol))
            return false;

        var rightSymbol = semanticModel.GetSymbolInfo(assignment.Right, ct).Symbol;
        return rightSymbol is IParameterSymbol { Name: "value" };
    }

    private static bool ContainsNameofFieldIdentifier(PropertyDeclarationSyntax property, string fieldName)
    {
        var typeDecl = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null)
            return false;

        foreach (var invocation in typeDecl.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is not IdentifierNameSyntax { Identifier.ValueText: "nameof" })
                continue;

            if (invocation.ArgumentList.Arguments.Any(a => a.ToString().Contains(fieldName)))
                return true;
        }

        return false;
    }

    private static bool ContainsAttributeFieldIdentifier(PropertyDeclarationSyntax property, string fieldName)
    {
        var typeDecl = property.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null)
            return false;

        foreach (var attribute in typeDecl.DescendantNodes().OfType<AttributeSyntax>())
        {
            if (attribute.ArgumentList is null)
                continue;

            if (attribute.ArgumentList.Arguments.Any(a => a.ToString().Contains(fieldName)))
                return true;
        }

        return false;
    }

    private static bool IsFieldReferencedOnlyWithinPropertyAccessors(
        SyntaxNode root,
        SemanticModel semanticModel,
        PropertyDeclarationSyntax property,
        IFieldSymbol fieldSymbol,
        CancellationToken ct)
    {
        var allowedNodes = new HashSet<SyntaxNode>();

        foreach (var accessor in property.AccessorList?.Accessors ?? default)
        {
            if (accessor.ExpressionBody is not null)
                allowedNodes.Add(accessor.ExpressionBody);

            if (accessor.Body is not null)
                allowedNodes.Add(accessor.Body);
        }

        var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        var anyUsage = false;

        foreach (var identifier in identifiers)
        {
            if (identifier.Identifier.ValueText != fieldSymbol.Name)
                continue;

            var symbol = semanticModel.GetSymbolInfo(identifier, ct).Symbol;
            if (!SymbolEqualityComparer.Default.Equals(symbol, fieldSymbol))
                continue;

            anyUsage = true;

            if (!allowedNodes.Any(a => a.Span.Contains(identifier.Span)))
                return false;
        }

        return anyUsage;
    }
}