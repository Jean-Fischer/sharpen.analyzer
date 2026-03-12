using System.Collections.Immutable;
using System.Linq;
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

        if (method.TypeParameterList is null || method.TypeParameterList.Parameters.Count == 0)
            return;

        if (method.ConstraintClauses.Count > 0)
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

        if (typeDecl.TypeParameterList is null || typeDecl.TypeParameterList.Parameters.Count == 0)
            return;

        if (typeDecl.ConstraintClauses.Count > 0)
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
        System.Threading.CancellationToken cancellationToken)
    {
        // Conservative heuristic:
        // - Any ref/out/in parameter of type T
        // - Any return type of ref T / ref readonly T
        // - Any field/property of type Span<T> / ReadOnlySpan<T>
        // - Any local/parameter of type Span<T> / ReadOnlySpan<T>
        //
        // This is guidance-only; we keep it intentionally narrow to avoid noise.

        foreach (var typeParameter in node.DescendantNodes().OfType<TypeParameterSyntax>())
        {
            var typeParamSymbol = semanticModel.GetDeclaredSymbol(typeParameter, cancellationToken) as ITypeParameterSymbol;
            if (typeParamSymbol is null)
                continue;

            // ref/out/in parameters
            foreach (var parameter in node.DescendantNodes().OfType<ParameterSyntax>())
            {
                if (parameter.Type is null)
                    continue;

                if (parameter.Modifiers.Any(SyntaxKind.RefKeyword)
                    || parameter.Modifiers.Any(SyntaxKind.OutKeyword)
                    || parameter.Modifiers.Any(SyntaxKind.InKeyword))
                {
                    var paramType = semanticModel.GetTypeInfo(parameter.Type, cancellationToken).Type;
                    if (SymbolEqualityComparer.Default.Equals(paramType, typeParamSymbol))
                        return true;
                }

                if (IsSpanOfTypeParameter(parameter.Type, typeParamSymbol, semanticModel, cancellationToken))
                    return true;
            }

            // return type
            if (node is MethodDeclarationSyntax method && method.ReturnType is not null)
            {
                if (method.ReturnType is RefTypeSyntax refType)
                {
                    var refReturnType = semanticModel.GetTypeInfo(refType.Type, cancellationToken).Type;
                    if (SymbolEqualityComparer.Default.Equals(refReturnType, typeParamSymbol))
                        return true;
                }

                if (IsSpanOfTypeParameter(method.ReturnType, typeParamSymbol, semanticModel, cancellationToken))
                    return true;
            }

            // properties/fields
            foreach (var memberType in node.DescendantNodes().OfType<BasePropertyDeclarationSyntax>())
            {
                if (memberType is PropertyDeclarationSyntax prop && prop.Type is not null)
                {
                    if (IsSpanOfTypeParameter(prop.Type, typeParamSymbol, semanticModel, cancellationToken))
                        return true;
                }

                if (memberType is IndexerDeclarationSyntax indexer && indexer.Type is not null)
                {
                    if (IsSpanOfTypeParameter(indexer.Type, typeParamSymbol, semanticModel, cancellationToken))
                        return true;
                }
            }

            foreach (var field in node.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                if (field.Declaration?.Type is null)
                    continue;

                if (IsSpanOfTypeParameter(field.Declaration.Type, typeParamSymbol, semanticModel, cancellationToken))
                    return true;
            }

            // locals
            foreach (var local in node.DescendantNodes().OfType<VariableDeclarationSyntax>())
            {
                if (local.Type is null)
                    continue;

                if (IsSpanOfTypeParameter(local.Type, typeParamSymbol, semanticModel, cancellationToken))
                    return true;
            }
        }

        return false;
    }

    private static bool IsSpanOfTypeParameter(
        TypeSyntax typeSyntax,
        ITypeParameterSymbol typeParameter,
        SemanticModel semanticModel,
        System.Threading.CancellationToken cancellationToken)
    {
        var type = semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type as INamedTypeSymbol;
        if (type is null)
            return false;

        if (type.TypeArguments.Length != 1)
            return false;

        if (!SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], typeParameter))
            return false;

        // Match Span<T> / ReadOnlySpan<T> by metadata name.
        return type.ContainingNamespace?.ToDisplayString() == "System"
            && (type.Name == "Span" || type.Name == "ReadOnlySpan");
    }
}
