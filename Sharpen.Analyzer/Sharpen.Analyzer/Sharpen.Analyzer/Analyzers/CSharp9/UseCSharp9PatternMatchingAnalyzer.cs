using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCSharp9PatternMatchingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseCSharp9PatternMatchingRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzePrefixUnaryExpression, SyntaxKind.LogicalNotExpression);
        context.RegisterSyntaxNodeAction(AnalyzeLogicalBinaryExpression, SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression);
    }

    private static bool IsCSharp9OrAbove(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.SyntaxTree.Options is not CSharpParseOptions parseOptions)
            return false;

        return parseOptions.LanguageVersion >= LanguageVersion.CSharp9;
    }

    private static bool IsSideEffectFree(ExpressionSyntax expression)
    {
        expression = expression.WalkDownParentheses();

        return expression is IdentifierNameSyntax
            or ThisExpressionSyntax
            or BaseExpressionSyntax
            or MemberAccessExpressionSyntax;
    }

    private static bool IsInExpressionTree(SyntaxNodeAnalysisContext context)
    {
        // Pattern matching ("is not null", relational patterns, etc.) is not supported in expression trees
        // (e.g. LINQ providers like EF Core), and will fail with:
        // "An expression tree may not contain an 'is' pattern-matching operator".
        //
        // So: if the current node is inside a lambda that is converted to Expression<...>, don't offer the diagnostic.
        var lambda = context.Node.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (lambda == null)
            return false;

        if (context.SemanticModel.GetTypeInfo(lambda, context.CancellationToken).ConvertedType is not INamedTypeSymbol convertedType)
            return false;

        return convertedType.Name == "Expression" &&
               convertedType.ContainingNamespace?.ToDisplayString() == "System.Linq.Expressions";
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!IsCSharp9OrAbove(context))
            return;

        if (IsInExpressionTree(context))
            return;

        if (context.Node is not BinaryExpressionSyntax binary)
            return;

        // expr != null  => expr is not null
        if (binary.IsKind(SyntaxKind.NotEqualsExpression)
            && binary.Right.IsKind(SyntaxKind.NullLiteralExpression)
            && IsSideEffectFree(binary.Left))
            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseCSharp9PatternMatchingRule,
                binary.GetLocation()));
    }

    private static void AnalyzePrefixUnaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!IsCSharp9OrAbove(context))
            return;

        if (IsInExpressionTree(context))
            return;

        if (context.Node is not PrefixUnaryExpressionSyntax prefix || !prefix.IsKind(SyntaxKind.LogicalNotExpression))
            return;

        // !(expr is T) => expr is not T
        if (prefix.Operand is not ParenthesizedExpressionSyntax paren) return;
        var inner = paren.Expression.WalkDownParentheses();
        if (inner is BinaryExpressionSyntax isExpr
            && isExpr.IsKind(SyntaxKind.IsExpression)
            && IsSideEffectFree(isExpr.Left))
            context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseCSharp9PatternMatchingRule,
                prefix.GetLocation()));
    }

    private static void AnalyzeLogicalBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (!IsCSharp9OrAbove(context))
            return;

        if (IsInExpressionTree(context))
            return;

        if (context.Node is not BinaryExpressionSyntax binary)
            return;

        // Range checks:
        // x >= a && x <= b
        // x < a || x > b
        if (binary.IsKind(SyntaxKind.LogicalAndExpression))
        {
            if (TryGetComparison(binary.Left, out var leftExpr, out var leftOp, out _)
                && TryGetComparison(binary.Right, out var rightExpr, out var rightOp, out _)
                && IsSideEffectFree(leftExpr)
                && SyntaxFactory.AreEquivalent(leftExpr.WalkDownParentheses(), rightExpr.WalkDownParentheses())
                && leftOp is SyntaxKind.GreaterThanOrEqualExpression
                && rightOp is SyntaxKind.LessThanOrEqualExpression)
                context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseCSharp9PatternMatchingRule,
                    binary.GetLocation()));
        }
        else if (binary.IsKind(SyntaxKind.LogicalOrExpression))
        {
            if (TryGetComparison(binary.Left, out var leftExpr, out var leftOp, out var leftBound)
                && TryGetComparison(binary.Right, out var rightExpr, out var rightOp, out var rightBound)
                && IsSideEffectFree(leftExpr)
                && SyntaxFactory.AreEquivalent(leftExpr.WalkDownParentheses(), rightExpr.WalkDownParentheses())
                && leftOp is SyntaxKind.LessThanExpression
                && rightOp is SyntaxKind.GreaterThanExpression
                // Relational patterns require a constant on the RHS. If bounds are not constants, we can't offer a safe fix.
                && leftBound.WalkDownParentheses() is LiteralExpressionSyntax
                && rightBound.WalkDownParentheses() is LiteralExpressionSyntax)
                context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseCSharp9PatternMatchingRule,
                    binary.GetLocation()));
        }
    }

    private static bool TryGetComparison(ExpressionSyntax expression, out ExpressionSyntax left, out SyntaxKind opKind,
        out ExpressionSyntax right)
    {
        left = null!;
        right = null!;
        opKind = default;

        expression = expression.WalkDownParentheses();
        if (expression is not BinaryExpressionSyntax binary)
            return false;

        if (!binary.IsKind(SyntaxKind.GreaterThanOrEqualExpression)
            && !binary.IsKind(SyntaxKind.LessThanOrEqualExpression)
            && !binary.IsKind(SyntaxKind.LessThanExpression)
            && !binary.IsKind(SyntaxKind.GreaterThanExpression)) return false;
        left = binary.Left;
        right = binary.Right;
        opKind = binary.Kind();
        return true;

    }
}