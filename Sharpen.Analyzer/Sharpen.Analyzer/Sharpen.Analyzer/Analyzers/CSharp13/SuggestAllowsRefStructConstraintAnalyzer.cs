using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuggestAllowsRefStructConstraintAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.SuggestAllowsRefStructConstraintRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.RecordDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;

        if (method.TypeParameterList?.Parameters.Any() != true)
            return;

        if (method.ConstraintClauses.Any())
            return;

        if (method.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            // Avoid suggesting on partial methods where constraints may be split across parts.
            return;
        }

        // Heuristic: if the method uses any of its type parameters in a byref-like position,
        // it may benefit from allowing ref struct type arguments.
        if (!UsesTypeParameterInByRefLikePosition(method, context.SemanticModel, context.CancellationToken))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.SuggestAllowsRefStructConstraintRule,
            method.Identifier.GetLocation()));
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;

        if (typeDecl.TypeParameterList?.Parameters.Any() != true)
            return;

        if (typeDecl.ConstraintClauses.Any())
            return;

        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            // Avoid suggesting on partial types where constraints may be split across parts.
            return;
        }

        if (!UsesTypeParameterInByRefLikePosition(typeDecl, context.SemanticModel, context.CancellationToken))
            return;

        context.ReportDiagnostic(Diagnostic.Create(
            CSharp13Rules.SuggestAllowsRefStructConstraintRule,
            typeDecl.Identifier.GetLocation()));
    }

    private static bool UsesTypeParameterInByRefLikePosition(
        SyntaxNode node,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Conservative heuristic:
        // - Any ref/out/in parameter of type T
        // - Any return type of ref T / ref readonly T
        // - Any field/property of type Span<T> / ReadOnlySpan<T>
        // - Any local/parameter of type Span<T> / ReadOnlySpan<T>
        //
        // This is guidance-only; we keep it intentionally narrow to avoid noise.

        return node.DescendantNodes()
            .OfType<TypeParameterSyntax>()
            .Select(typeParameter => semanticModel.GetDeclaredSymbol(typeParameter, cancellationToken))
            .OfType<ITypeParameterSymbol>()
            .Any(typeParameter => UsesTypeParameterInByRefLikePosition(node, typeParameter, semanticModel, cancellationToken));
    }

    private static bool UsesTypeParameterInByRefLikePosition(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return HasByRefParameterUsage(node, typeParameter, semanticModel, cancellationToken)
               || HasSpanParameterUsage(node, typeParameter, semanticModel, cancellationToken)
               || HasMethodReturnUsage(node, typeParameter, semanticModel, cancellationToken)
               || HasPropertyOrIndexerUsage(node, typeParameter, semanticModel, cancellationToken)
               || HasFieldOrLocalUsage(node, typeParameter, semanticModel, cancellationToken);
    }

    private static bool HasByRefParameterUsage(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var parameter in node.DescendantNodes().OfType<ParameterSyntax>())
        {
            if (parameter.Type is null || !HasByRefModifier(parameter))
                continue;

            var parameterType = semanticModel.GetTypeInfo(parameter.Type, cancellationToken).Type;
            if (SymbolEqualityComparer.Default.Equals(parameterType, typeParameter))
                return true;
        }

        return false;
    }

    private static bool HasSpanParameterUsage(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return node.DescendantNodes()
            .OfType<ParameterSyntax>()
            .Where(parameter => parameter.Type is not null)
            .Any(parameter => IsSpanOfTypeParameter(parameter.Type!, typeParameter, semanticModel, cancellationToken));
    }

    private static bool HasMethodReturnUsage(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (node is not MethodDeclarationSyntax method)
            return false;

        if (method.ReturnType is RefTypeSyntax refType)
        {
            var refReturnType = semanticModel.GetTypeInfo(refType.Type, cancellationToken).Type;
            if (SymbolEqualityComparer.Default.Equals(refReturnType, typeParameter))
                return true;
        }

        return IsSpanOfTypeParameter(method.ReturnType, typeParameter, semanticModel, cancellationToken);
    }

    private static bool HasPropertyOrIndexerUsage(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var member in node.DescendantNodes().OfType<BasePropertyDeclarationSyntax>())
        {
            var typeSyntax = member switch
            {
                PropertyDeclarationSyntax property => property.Type,
                IndexerDeclarationSyntax indexer => indexer.Type,
                _ => null
            };

            if (typeSyntax != null && IsSpanOfTypeParameter(typeSyntax, typeParameter, semanticModel, cancellationToken))
                return true;
        }

        return false;
    }

    private static bool HasFieldOrLocalUsage(
        SyntaxNode node,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return node.DescendantNodes().OfType<FieldDeclarationSyntax>()
                   .Any(field => IsSpanOfTypeParameter(field.Declaration.Type, typeParameter, semanticModel, cancellationToken))
               || node.DescendantNodes().OfType<VariableDeclarationSyntax>()
                   .Any(local => IsSpanOfTypeParameter(local.Type, typeParameter, semanticModel, cancellationToken));
    }

    private static bool HasByRefModifier(ParameterSyntax parameter)
    {
        return parameter.Modifiers.Any(SyntaxKind.RefKeyword)
               || parameter.Modifiers.Any(SyntaxKind.OutKeyword)
               || parameter.Modifiers.Any(SyntaxKind.InKeyword);
    }

    private static bool IsSpanOfTypeParameter(
        TypeSyntax typeSyntax,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type is not INamedTypeSymbol type)
            return false;

        if (type.TypeArguments.Length != 1)
            return false;

        if (!SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], typeParameter))
            return false;

        // Match Span<T> / ReadOnlySpan<T> by metadata name.
        return type.ContainingNamespace?.ToDisplayString() == "System"
               && type.Name is "Span" or "ReadOnlySpan";
    }
}
