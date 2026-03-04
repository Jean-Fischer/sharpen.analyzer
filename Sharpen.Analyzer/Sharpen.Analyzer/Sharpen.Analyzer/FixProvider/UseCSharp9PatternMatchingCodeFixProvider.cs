using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.FixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCSharp9PatternMatchingCodeFixProvider))]
[Shared]
public sealed class UseCSharp9PatternMatchingCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseCSharp9PatternMatchingRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp9OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use C# 9 pattern matching",
                createChangedDocument: c => ApplyFixAsync(context.Document, node, c),
                equivalenceKey: "UseCSharp9PatternMatching"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp9OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    private static async Task<Document> ApplyFixAsync(Document document, SyntaxNode node, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root == null)
            return document;

        SyntaxNode? replacement = node switch
        {
            BinaryExpressionSyntax binary => TryRewriteBinary(binary),
            PrefixUnaryExpressionSyntax prefix => TryRewritePrefix(prefix),
            _ => null
        };

        if (replacement == null)
            return document;

        var newRoot = root.ReplaceNode(node, replacement);
        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax? TryRewriteBinary(BinaryExpressionSyntax binary)
    {
        // expr != null => expr is not null
        if (binary.IsKind(SyntaxKind.NotEqualsExpression)
            && binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            var expr = binary.Left.WithoutTrivia();
            var pattern = SyntaxFactory.UnaryPattern(
                SyntaxFactory.Token(SyntaxKind.NotKeyword),
                SyntaxFactory.ConstantPattern(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)));

            var isPattern = SyntaxFactory.IsPatternExpression(expr, pattern)
                .WithTriviaFrom(binary);

            return isPattern;
        }

        // Range checks
        if (binary.IsKind(SyntaxKind.LogicalAndExpression)
            && TryGetComparison(binary.Left, out var leftExpr, out var leftOp, out var leftBound)
            && TryGetComparison(binary.Right, out var rightExpr, out var rightOp, out var rightBound)
            && SyntaxFactory.AreEquivalent(leftExpr.WalkDownParentheses(), rightExpr.WalkDownParentheses())
            && leftOp is SyntaxKind.GreaterThanOrEqualExpression
            && rightOp is SyntaxKind.LessThanOrEqualExpression)
        {
            var expr = leftExpr.WithoutTrivia();
            var leftPattern = SyntaxFactory.RelationalPattern(SyntaxFactory.Token(SyntaxKind.GreaterThanEqualsToken), leftBound.WithoutTrivia());
            var rightPattern = SyntaxFactory.RelationalPattern(SyntaxFactory.Token(SyntaxKind.LessThanEqualsToken), rightBound.WithoutTrivia());

            // Build: x is >= a and <= b
            var isExpression = SyntaxFactory.ParseExpression($"{expr.WithoutTrivia()} is >= {leftBound.WithoutTrivia()} and <= {rightBound.WithoutTrivia()}");
            return isExpression.WithTriviaFrom(binary);
        }

        if (binary.IsKind(SyntaxKind.LogicalOrExpression)
            && TryGetComparison(binary.Left, out var leftExpr2, out var leftOp2, out var leftBound2)
            && TryGetComparison(binary.Right, out var rightExpr2, out var rightOp2, out var rightBound2)
            && SyntaxFactory.AreEquivalent(leftExpr2.WalkDownParentheses(), rightExpr2.WalkDownParentheses())
            && leftOp2 is SyntaxKind.LessThanExpression
            && rightOp2 is SyntaxKind.GreaterThanExpression)
        {
            // NOTE: Relational patterns require a constant on the RHS. In the general case (a/b are variables),
            // rewriting to pattern matching would produce code that doesn't compile (CS0150).
            // So we intentionally do NOT offer a code fix for this form.
            return null;
        }

        return null;
    }

    private static ExpressionSyntax? TryRewritePrefix(PrefixUnaryExpressionSyntax prefix)
    {
        // !(expr is T) => expr is not T
        if (!prefix.IsKind(SyntaxKind.LogicalNotExpression))
            return null;

        if (prefix.Operand is not ParenthesizedExpressionSyntax paren)
            return null;

        var inner = paren.Expression.WalkDownParentheses();
        if (inner is not BinaryExpressionSyntax isExpr || !isExpr.IsKind(SyntaxKind.IsExpression))
            return null;

        var expr = isExpr.Left.WithoutTrivia();
        var type = isExpr.Right.WithoutTrivia();

        var pattern = SyntaxFactory.UnaryPattern(
            SyntaxFactory.Token(SyntaxKind.NotKeyword),
            SyntaxFactory.TypePattern((TypeSyntax)type));

        return SyntaxFactory.IsPatternExpression(expr, pattern).WithTriviaFrom(prefix);
    }

    private static bool TryGetComparison(ExpressionSyntax expression, out ExpressionSyntax left, out SyntaxKind opKind, out ExpressionSyntax right)
    {
        left = null!;
        right = null!;
        opKind = default;

        expression = expression.WalkDownParentheses();
        if (expression is not BinaryExpressionSyntax binary)
            return false;

        if (binary.IsKind(SyntaxKind.GreaterThanOrEqualExpression)
            || binary.IsKind(SyntaxKind.LessThanOrEqualExpression)
            || binary.IsKind(SyntaxKind.LessThanExpression)
            || binary.IsKind(SyntaxKind.GreaterThanExpression))
        {
            left = binary.Left;
            right = binary.Right;
            opKind = binary.Kind();
            return true;
        }

        return false;
    }
}
