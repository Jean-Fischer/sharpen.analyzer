using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp3;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseDefaultExpressionInReturnStatementsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseDefaultExpressionInReturnStatementsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDefaultExpression, SyntaxKind.DefaultExpression);
    }

    private static void AnalyzeDefaultExpression(SyntaxNodeAnalysisContext context)
    {
        var defaultExpression = (DefaultExpressionSyntax)context.Node;

        if (defaultExpression.Parent is not ReturnStatementSyntax returnStatement) return;

        var defaultExpressionType = context.SemanticModel.GetTypeInfo(defaultExpression).Type;
        if (defaultExpressionType is null) return;

        var enclosingReturnType = GetEnclosingDeclarationReturnType(defaultExpression, context.SemanticModel);
        if (enclosingReturnType is null) return;

        if (!SymbolEqualityComparer.Default.Equals(defaultExpressionType, enclosingReturnType)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.Rules.UseDefaultExpressionInReturnStatementsRule,
            defaultExpression.GetLocation(),
            defaultExpressionType.ToDisplayString()));
    }

    private static ITypeSymbol? GetEnclosingDeclarationReturnType(DefaultExpressionSyntax expression, SemanticModel semanticModel)
    {
        var method = expression.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method != null) return semanticModel.GetDeclaredSymbol(method)?.ReturnType;

        var @operator = expression.FirstAncestorOrSelf<OperatorDeclarationSyntax>();
        if (@operator != null) return semanticModel.GetDeclaredSymbol(@operator)?.ReturnType;

        var conversion = expression.FirstAncestorOrSelf<ConversionOperatorDeclarationSyntax>();
        if (conversion != null) return semanticModel.GetDeclaredSymbol(conversion)?.ReturnType;

        var property = expression.FirstAncestorOrSelf<BasePropertyDeclarationSyntax>();
        if (property != null) return (semanticModel.GetDeclaredSymbol(property) as IPropertySymbol)?.Type;

        return null;
    }
}
