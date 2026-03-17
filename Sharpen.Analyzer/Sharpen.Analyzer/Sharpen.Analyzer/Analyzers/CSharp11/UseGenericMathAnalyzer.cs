using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseGenericMathAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp11Rules.UseGenericMathRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp11OrAbove(compilationContext.Compilation))
                return;

            // Only run if System.Numerics.INumber`1 is available.
            var iNumber = compilationContext.Compilation.GetTypeByMetadataName("System.Numerics.INumber`1");
            if (iNumber == null)
                return;

            compilationContext.RegisterSyntaxNodeAction(
                ctx => AnalyzeBinaryExpression(ctx, iNumber),
                SyntaxKind.AddExpression,
                SyntaxKind.SubtractExpression,
                SyntaxKind.MultiplyExpression,
                SyntaxKind.DivideExpression,
                SyntaxKind.ModuloExpression);
        });
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol iNumber)
    {
        var binary = (BinaryExpressionSyntax)context.Node;

        // Only consider expressions inside a generic method.
        var method = binary.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.TypeParameterList == null)
            return;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method, context.CancellationToken);
        if (methodSymbol == null)
            return;

        // Determine if either side is a type parameter.
        var leftType = context.SemanticModel.GetTypeInfo(binary.Left, context.CancellationToken).Type;
        var rightType = context.SemanticModel.GetTypeInfo(binary.Right, context.CancellationToken).Type;

        var typeParam = leftType as ITypeParameterSymbol ?? rightType as ITypeParameterSymbol;
        if (typeParam == null)
            return;

        // Ensure the type parameter belongs to this method.
        if (!methodSymbol.TypeParameters.Contains(typeParam, SymbolEqualityComparer.Default))
            return;

        // Suppress if a compatible INumber<T> constraint already exists.
        if (HasINumberConstraint(typeParam, iNumber))
            return;

        var diagnostic = Diagnostic.Create(
            CSharp11Rules.UseGenericMathRule,
            binary.OperatorToken.GetLocation(),
            typeParam.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasINumberConstraint(ITypeParameterSymbol typeParam, INamedTypeSymbol iNumber)
    {
        foreach (var constraint in typeParam.ConstraintTypes)
        {
            if (constraint is not INamedTypeSymbol named)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, iNumber))
                continue;

            // INumber<TSelf> where TSelf is the type parameter itself.
            if (named.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(named.TypeArguments[0], typeParam))
                return true;
        }

        return false;
    }
}