using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuggestCompoundAssignmentOperatorsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.SuggestCompoundAssignmentOperatorsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp14OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeAddAssignment, SyntaxKind.AddAssignmentExpression);
        });
    }

    private static void AnalyzeAddAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignment)
            return;

        // We only care about user-defined operators; primitives already have built-in compound assignment.
        var leftType = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken).Type;
        if (leftType is null)
            return;

        if (leftType.SpecialType != SpecialType.None)
            return;

        // Only consider named types (struct/class/record). Skip type parameters, dynamic, etc.
        if (leftType is not INamedTypeSymbol namedType)
            return;

        // If the type already defines operator +=, no suggestion.
        if (HasCompoundAssignmentOperator(namedType))
            return;

        // Suggest only when a binary + operator exists.
        if (!HasBinaryPlusOperator(namedType))
            return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                CSharp14Rules.SuggestCompoundAssignmentOperatorsRule,
                assignment.OperatorToken.GetLocation(),
                namedType.Name));
    }

    private static bool HasBinaryPlusOperator(INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers("op_Addition"))
        {
            if (member is not IMethodSymbol method)
                continue;

            if (!method.IsStatic)
                continue;

            if (method.Parameters.Length != 2)
                continue;

            // We only require that the operator is declared on the type.
            return true;
        }

        return false;
    }

    private static bool HasCompoundAssignmentOperator(INamedTypeSymbol type)
    {
        // C# represents user-defined compound assignment operators as op_AdditionAssignment, etc.
        foreach (var member in type.GetMembers("op_AdditionAssignment"))
        {
            if (member is IMethodSymbol { IsStatic: true })
                return true;
        }

        return false;
    }
}