using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseNullConditionalAssignmentAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseNullConditionalAssignmentRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (!TryGetNullCheckedExpression(ifStatement.Condition, out var checkedExpression))
            return;

        if (!TryGetSingleStatement(ifStatement.Statement, out var singleStatement))
            return;

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } expressionStatement)
            return;

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return;

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (!AreEquivalentIgnoringParentheses(checkedExpression, memberAccess.Expression))
            return;

        // Only support simple member access: x.Member = rhs;
        // (No element access, no conditional access, no compound assignments.)
        context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseNullConditionalAssignmentRule, ifStatement.GetLocation()));
    }

    private static bool TryGetNullCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax checkedExpression)
    {
        checkedExpression = null!;

        // Support: x != null
        if (condition is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (IsNullLiteral(binary.Right))
            {
                checkedExpression = binary.Left;
                return true;
            }

            if (IsNullLiteral(binary.Left))
            {
                checkedExpression = binary.Right;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetSingleStatement(StatementSyntax statement, out StatementSyntax singleStatement)
    {
        if (statement is BlockSyntax block)
        {
            if (block.Statements.Count == 1)
            {
                singleStatement = block.Statements[0];
                return true;
            }

            singleStatement = null!;
            return false;
        }

        singleStatement = statement;
        return true;
    }

    private static bool IsNullLiteral(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool AreEquivalentIgnoringParentheses(ExpressionSyntax left, ExpressionSyntax right)
    {
        left = StripParentheses(left);
        right = StripParentheses(right);

        return SyntaxFactory.AreEquivalent(left, right);
    }

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
