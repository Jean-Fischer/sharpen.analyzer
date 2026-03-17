using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseListPatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp11Rules.UseListPatternRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp11OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeIfStatement, SyntaxKind.IfStatement);
        });
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        // Pattern: if (x.Length > 0) { ... x[0] ... }
        if (ifStatement.Condition is not BinaryExpressionSyntax condition)
            return;

        if (!condition.IsKind(SyntaxKind.GreaterThanExpression) && !condition.IsKind(SyntaxKind.NotEqualsExpression))
            return;

        if (!TryGetLengthCheck(condition, out var targetExpression))
            return;

        // Ensure target is array/span-like.
        var targetType = context.SemanticModel.GetTypeInfo(targetExpression, context.CancellationToken).Type;
        if (!IsArrayOrSpanLike(targetType))
            return;

        // Ensure body contains an indexer access [0] on the same target.
        if (!ContainsZeroIndexAccess(context, ifStatement.Statement, targetExpression))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp11Rules.UseListPatternRule,
            ifStatement.IfKeyword.GetLocation()));
    }

    private static bool TryGetLengthCheck(BinaryExpressionSyntax condition, out ExpressionSyntax target)
    {
        target = null!;

        // x.Length > 0
        if (condition.Left is MemberAccessExpressionSyntax leftMember
            && leftMember.Name.Identifier.ValueText == "Length"
            && condition.Right is LiteralExpressionSyntax rightLiteral
            && rightLiteral.Token.ValueText == "0")
        {
            target = leftMember.Expression;
            return true;
        }

        // 0 < x.Length
        if (condition.Right is not MemberAccessExpressionSyntax rightMember
            || rightMember.Name.Identifier.ValueText != "Length"
            || condition.Left is not LiteralExpressionSyntax leftLiteral
            || leftLiteral.Token.ValueText != "0") return false;
        target = rightMember.Expression;
        return true;

    }

    private static bool IsArrayOrSpanLike(ITypeSymbol? type)
    {
        switch (type)
        {
            case null:
                return false;
            case IArrayTypeSymbol:
            // Span<T> / ReadOnlySpan<T>
            case INamedTypeSymbol named when (named.ContainingNamespace?.ToDisplayString() == "System" &&
                                              named.Name is "Span" or "ReadOnlySpan" &&
                                              named.TypeArguments.Length == 1):
                return true;
            default:
                return false;
        }
    }

    private static bool ContainsZeroIndexAccess(SyntaxNodeAnalysisContext context, StatementSyntax statement,
        ExpressionSyntax targetExpression)
    {
        foreach (var elementAccess in statement.DescendantNodes().OfType<ElementAccessExpressionSyntax>())
        {
            if (elementAccess.ArgumentList.Arguments.Count != 1)
                continue;

            var arg = elementAccess.ArgumentList.Arguments[0].Expression;
            if (arg is not LiteralExpressionSyntax literal || literal.Token.ValueText != "0")
                continue;

            // Compare symbols for the target expression.
            var elementTargetSymbol = context.SemanticModel
                .GetSymbolInfo(elementAccess.Expression, context.CancellationToken).Symbol;
            var targetSymbol = context.SemanticModel.GetSymbolInfo(targetExpression, context.CancellationToken).Symbol;

            if (elementTargetSymbol != null && targetSymbol != null &&
                SymbolEqualityComparer.Default.Equals(elementTargetSymbol, targetSymbol))
                return true;
        }

        return false;
    }
}