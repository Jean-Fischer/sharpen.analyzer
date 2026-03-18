using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp7;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseVarKeywordAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseVarKeywordRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
    }

    private void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (VariableDeclarationSyntax)context.Node;
        if (declaration.Parent is not (LocalDeclarationStatementSyntax or UsingStatementSyntax))
            return;

        if (declaration.Type.IsVar)
            return;

        if (declaration.Variables.Count != 1)
            return;

        var variable = declaration.Variables[0];
        if (variable.Initializer?.Value.IsKind(SyntaxKind.ObjectCreationExpression) != true)
            return;

        var semanticModel = context.SemanticModel;
        var leftSideType = semanticModel.GetTypeInfo(declaration.Type).Type;
        var objectCreation = (ObjectCreationExpressionSyntax)variable.Initializer.Value;
        var rightSideType = semanticModel.GetTypeInfo(objectCreation).Type;

        if (leftSideType == null || rightSideType == null || leftSideType is IErrorTypeSymbol ||
            rightSideType is IErrorTypeSymbol)
        {
            return;
        }

        if (!SymbolEqualityComparer.Default.Equals(leftSideType, rightSideType))
            return;

        var diagnostic = Diagnostic.Create(
            Rules.Rules.UseVarKeywordRule,
            declaration.Type.GetLocation(),
            leftSideType.ToDisplayString()
        );
        context.ReportDiagnostic(diagnostic);
    }
}