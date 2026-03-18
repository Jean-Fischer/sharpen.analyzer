using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.Analyzers.CSharp10;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseInterpolatedStringAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            CSharp10Rules.UseInterpolatedStringRule,
            CSharp10Rules.UseConstInterpolatedStringRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAddExpression, SyntaxKind.AddExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var invocation = (InvocationExpressionSyntax)context.Node;

        // Detect string.Format("...", ...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol is not IMethodSymbol symbol)
            return;

        if (symbol.Name != "Format")
            return;

        if (symbol.ContainingType.SpecialType != SpecialType.System_String)
            return;

        if (invocation.ArgumentList.Arguments.Count < 1)
            return;

        if (invocation.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax literal ||
            !literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        // Basic placeholder detection: {0}, {1:000}, etc.
        var text = literal.Token.ValueText;
        if (!text.Contains("{", StringComparison.Ordinal))
            return;

        Report(context, invocation);
    }

    private static void AnalyzeAddExpression(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var add = (BinaryExpressionSyntax)context.Node;

        // Detect concatenation chains that include at least one string literal.
        if (!IsStringConcatenation(add, context.SemanticModel, context.CancellationToken))
            return;

        // Only report on the top-most concatenation node.
        if (add.Parent is BinaryExpressionSyntax parent && parent.IsKind(SyntaxKind.AddExpression))
            return;

        Report(context, add);
    }

    private static void Report(SyntaxNodeAnalysisContext context, ExpressionSyntax expr)
    {
        if (!IsSafeToReport(context, expr))
            return;

        // If this expression is assigned to a const string, also report the const-specific diagnostic.
        if (IsConstStringAssignment(expr, context.SemanticModel, context.CancellationToken))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(CSharp10Rules.UseConstInterpolatedStringRule, expr.GetLocation()));
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(CSharp10Rules.UseInterpolatedStringRule, expr.GetLocation()));
    }

    private static bool IsSafeToReport(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        // NOTE: This analyzer must not reference concrete fix provider types.
        // The global safety gate is still applied, but local per-fix-provider checks are evaluated
        // in the code-fix path.
        var evaluation = FixProviderSafetyRunner.Evaluate(
            context.SemanticModel,
            typeof(object),
            expression,
            null,
            context.CancellationToken);

        return evaluation.Outcome == FixProviderSafetyOutcome.Safe;
    }

    private static bool IsConstStringAssignment(ExpressionSyntax expr, SemanticModel model, CancellationToken ct)
    {
        // const string S = <expr>;
        if (expr.Parent is not EqualsValueClauseSyntax equalsValue)
            return false;

        if (equalsValue.Parent is not VariableDeclaratorSyntax declarator)
            return false;

        if (declarator.Parent is not VariableDeclarationSyntax declaration)
            return false;

        switch (declaration.Parent)
        {
            // const local
            case LocalDeclarationStatementSyntax local when !local.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)):
                return false;
            case LocalDeclarationStatementSyntax:
            {
                var type = model.GetTypeInfo(declaration.Type, ct).ConvertedType;
                return type != null && type.SpecialType == SpecialType.System_String;
            }
            // const field
            case FieldDeclarationSyntax field when !field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)):
                return false;
            case FieldDeclarationSyntax:
            {
                var type = model.GetTypeInfo(declaration.Type, ct).ConvertedType;
                return type is { SpecialType: SpecialType.System_String };
            }
            default:
                return false;
        }
    }


    private static bool IsStringConcatenation(BinaryExpressionSyntax add, SemanticModel model, CancellationToken ct)
    {
        // Ensure the overall type is string.
        var type = model.GetTypeInfo(add, ct).ConvertedType;
        if (type is not { SpecialType: SpecialType.System_String })
            return false;

        // Require at least one string literal in the chain.
        return FlattenAdd(add)
            .Any(e => e is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression));
    }

    private static IEnumerable<ExpressionSyntax> FlattenAdd(ExpressionSyntax expr)
    {
        while (true)
        {
            if (expr is BinaryExpressionSyntax bin && bin.IsKind(SyntaxKind.AddExpression))
            {
                foreach (var e in FlattenAdd(bin.Left)) yield return e;
                expr = bin.Right;
                continue;
            }

            yield return expr;
            break;
        }
    }
}