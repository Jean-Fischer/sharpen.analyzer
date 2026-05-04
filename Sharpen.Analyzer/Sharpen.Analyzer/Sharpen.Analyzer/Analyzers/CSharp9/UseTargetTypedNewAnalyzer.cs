using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseTargetTypedNewAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseTargetTypedNewRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static bool IsCSharp9OrAbove(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.SyntaxTree.Options is not CSharpParseOptions parseOptions)
            return false;

        return parseOptions.LanguageVersion >= LanguageVersion.CSharp9;
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (!IsCSharp9OrAbove(context) || context.Node is not ObjectCreationExpressionSyntax objectCreation)
            return;

        var createdType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
        if (createdType is null)
            return;

        // Don't touch arrays/implicit object creation (already target-typed) etc.
        if (TryReportForInitializer(context, objectCreation, createdType))
            return;

        if (TryReportForAssignment(context, objectCreation, createdType))
            return;

        TryReportForReturn(context, objectCreation, createdType);
    }

    private static bool TryReportForInitializer(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol createdType)
    {
        if (objectCreation.Parent is not EqualsValueClauseSyntax equalsValue)
            return false;

        return equalsValue.Parent switch
        {
            VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax variableDeclaration
            } when variableDeclaration.Type is not IdentifierNameSyntax { Identifier.ValueText: "var" } =>
                TryReportIfTypesMatch(context, objectCreation, variableDeclaration.Type, createdType),
            PropertyDeclarationSyntax propertyDeclaration =>
                TryReportIfTypesMatch(context, objectCreation, propertyDeclaration.Type, createdType),
            FieldDeclarationSyntax fieldDeclaration =>
                TryReportIfTypesMatch(context, objectCreation, fieldDeclaration.Declaration.Type, createdType),
            _ => false
        };
    }

    private static bool TryReportForAssignment(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol createdType)
    {
        if (objectCreation.Parent is not AssignmentExpressionSyntax assignment
            || !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            return false;
        }

        var leftType = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken).Type;
        return TryReportIfTypesMatch(context, objectCreation, leftType, createdType);
    }

    private static void TryReportForReturn(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol createdType)
    {
        if (objectCreation.Parent is not ReturnStatementSyntax returnStatement)
            return;

        if (context.SemanticModel.GetEnclosingSymbol(returnStatement.SpanStart, context.CancellationToken) is not IMethodSymbol method)
            return;

        TryReportIfTypesMatch(context, objectCreation, method.ReturnType, createdType);
    }

    private static bool TryReportIfTypesMatch(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        TypeSyntax targetTypeSyntax,
        ITypeSymbol createdType)
    {
        var targetType = context.SemanticModel.GetTypeInfo(targetTypeSyntax, context.CancellationToken).Type;
        return TryReportIfTypesMatch(context, objectCreation, targetType, createdType);
    }

    private static bool TryReportIfTypesMatch(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol? targetType,
        ITypeSymbol createdType)
    {
        if (targetType == null || !SymbolEqualityComparer.Default.Equals(targetType, createdType))
            return false;

        context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseTargetTypedNewRule,
            objectCreation.GetLocation()));
        return true;
    }
}
