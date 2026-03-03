using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Rules.Rules.UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule,
            Rules.Rules.ConsiderUsingNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCoalesceExpression, SyntaxKind.CoalesceExpression);
    }

    private static void AnalyzeCoalesceExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax coalesceExpression)
            return;

        if (coalesceExpression.Parent is not AssignmentExpressionSyntax assignment)
            return;

        if (assignment.Kind() != SyntaxKind.SimpleAssignmentExpression)
            return;

        if (!ReferenceEquals(assignment.Right, coalesceExpression))
            return;

        if (assignment.Left is null || coalesceExpression.Left is null)
            return;

        // Ignore situations like: X = X ?? throw new Exception().
        if (coalesceExpression.Right?.IsKind(SyntaxKind.ThrowExpression) == true)
            return;

        // Don't offer suggestion if this is `X = X ?? ...` inside an object initializer like e.g. `new SomeClass { X = X ?? ... }`.
        if (assignment.Left is IdentifierNameSyntax identifierName &&
            identifierName.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax { Parent: ObjectCreationExpressionSyntax } })
        {
            return;
        }

        if (!SyntaxNodeFacts.AreEquivalent(assignment.Left, coalesceExpression.Left))
            return;

        var isSideEffectFree = IsSideEffectFree(context, assignment.Left, isTopLevel: true);

        var rule = isSideEffectFree
            ? Rules.Rules.UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule
            : Rules.Rules.ConsiderUsingNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule;

        context.ReportDiagnostic(Diagnostic.Create(rule, assignment.GetLocation()));
    }

    // Ported from legacy Sharpen analyzer. This is intentionally conservative.
    private static bool IsSideEffectFree(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, bool isTopLevel)
    {
        // Referencing this or base like e.g. this.a.b.c causes no side effects itself.
        if (expression.IsKind(SyntaxKind.ThisExpression) || expression.IsKind(SyntaxKind.BaseExpression))
            return true;

        if (expression is IdentifierNameSyntax)
            return IsSideEffectFreeSymbol(context, expression, isTopLevel);

        if (expression is ParenthesizedExpressionSyntax parenthesized)
            return IsSideEffectFree(context, parenthesized.Expression, isTopLevel);

        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            return IsSideEffectFree(context, memberAccess.Expression, isTopLevel: false) &&
                   IsSideEffectFreeSymbol(context, memberAccess, isTopLevel);
        }

        if (expression is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            // `a?.b` is represented as ConditionalAccessExpression(accessExpression, whenNotNull)
            if (conditionalAccess.Expression is not ExpressionSyntax accessExpression)
                return false;

            if (conditionalAccess.WhenNotNull is not ExpressionSyntax whenNotNull)
                return false;

            return IsSideEffectFree(context, accessExpression, isTopLevel: false) &&
                   IsSideEffectFree(context, whenNotNull, isTopLevel: false);
        }

        // Something we don't explicitly handle. Assume this may have side effects.
        return false;
    }

    private static bool IsSideEffectFreeSymbol(SyntaxNodeAnalysisContext context, SyntaxNode node, bool isTopLevelNode)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(node, context.CancellationToken);
        if (symbolInfo.CandidateSymbols.Length > 0 || symbolInfo.Symbol is null)
        {
            // Couldn't bind successfully, assume that this might have side-effects.
            return false;
        }

        var symbol = symbolInfo.Symbol;
        switch (symbol.Kind)
        {
            case SymbolKind.Namespace:
            case SymbolKind.NamedType:
            case SymbolKind.Field:
            case SymbolKind.Parameter:
            case SymbolKind.Local:
                return true;
        }

        if (symbol.Kind == SymbolKind.Property && isTopLevelNode)
        {
            // Legacy analyzer had a TODO about ref-properties. We keep the same conservative behavior.
            // If this repository's Roslyn version exposes ReturnsByRef, we could refine this later.
            return true;
        }

        return false;
    }
}
