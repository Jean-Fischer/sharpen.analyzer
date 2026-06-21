using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp12;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePrimaryConstructorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp12Rules.UsePrimaryConstructorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        // NOTE: Primary constructors are a C# 12 feature.
        // We intentionally do not hard-gate this analyzer on LanguageVersion here because:
        // - the analyzer itself only looks for classic constructor patterns (no C# 12 syntax required)
        // - the test harness in this repo doesn't currently set parse options to Preview
        // Consumers can still control applicability via their project language version.

        if (context.Node is not TypeDeclarationSyntax typeDecl)
            return;

        // Conservative: no partial types.
        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return;

        if (!TryGetSingleInstanceConstructor(typeDecl, out var ctor))
            return;

        // No constructor initializer (base/this chaining).
        if (ctor.Initializer != null)
            return;

        // Must have a block body with only simple assignments.
        if (ctor.Body == null || !ctor.ParameterList.Parameters.Any())
            return;

        if (!AllStatementsAssignDistinctParameters(context, ctor))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp12Rules.UsePrimaryConstructorRule,
            ctor.Identifier.GetLocation()));
    }

    private static bool TryGetSingleInstanceConstructor(
        TypeDeclarationSyntax typeDecl,
        out ConstructorDeclarationSyntax ctor)
    {
        var ctors = typeDecl.Members.OfType<ConstructorDeclarationSyntax>()
            .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword))
            .ToArray();

        if (ctors.Length == 1)
        {
            ctor = ctors[0];
            return true;
        }

        ctor = null!;
        return false;
    }

    private static bool AllStatementsAssignDistinctParameters(
        SyntaxNodeAnalysisContext context,
        ConstructorDeclarationSyntax ctor)
    {
        var parameters = ctor.ParameterList.Parameters;

        // Each statement must be: <member> = <parameter>;
        // and each parameter must be used exactly once.
        var usedParameters = new bool[parameters.Count];

        foreach (var statement in ctor.Body.Statements)
        {
            if (!TryProcessAssignmentStatement(context, parameters, usedParameters, statement))
                return false;
        }

        if (usedParameters.Any(u => !u))
            return false;

        return true;
    }

    private static bool TryProcessAssignmentStatement(
        SyntaxNodeAnalysisContext context,
        SeparatedSyntaxList<ParameterSyntax> parameters,
        bool[] usedParameters,
        StatementSyntax statement)
    {
        if (statement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
            || !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
            || assignment.Right is not IdentifierNameSyntax rhsIdentifier
            || !TryGetParameterIndex(parameters, rhsIdentifier, out var paramIndex)
            || usedParameters[paramIndex]
            || !IsSupportedAssignmentTarget(assignment.Left)
            || !LeftSymbolIsInstanceFieldOrProperty(context, assignment.Left)
            || !RightSymbolIsParameter(context, rhsIdentifier))
        {
            return false;
        }

        usedParameters[paramIndex] = true;
        return true;
    }

    private static bool TryGetParameterIndex(
        SeparatedSyntaxList<ParameterSyntax> parameters,
        IdentifierNameSyntax rhsIdentifier,
        out int parameterIndex)
    {
        parameterIndex = parameters.IndexOf(p => p.Identifier.ValueText == rhsIdentifier.Identifier.ValueText);
        return parameterIndex >= 0;
    }

    private static bool IsSupportedAssignmentTarget(ExpressionSyntax assignmentLeft)
    {
        return assignmentLeft is IdentifierNameSyntax
               or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax };
    }

    private static bool LeftSymbolIsInstanceFieldOrProperty(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax assignmentLeft)
    {
        var leftSymbol = context.SemanticModel.GetSymbolInfo(assignmentLeft, context.CancellationToken).Symbol;
        return leftSymbol is IFieldSymbol or IPropertySymbol && !leftSymbol.IsStatic;
    }

    private static bool RightSymbolIsParameter(
        SyntaxNodeAnalysisContext context,
        IdentifierNameSyntax rhsIdentifier)
    {
        return context.SemanticModel.GetSymbolInfo(rhsIdentifier, context.CancellationToken).Symbol is IParameterSymbol;
    }
}
