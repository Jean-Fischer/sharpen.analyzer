using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp10;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExtendedPropertyPatternCodeFixProvider))]
[Shared]
public sealed class UseExtendedPropertyPatternCodeFixProvider : CSharp10OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseExtendedPropertyPatternRule.Id);

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Two supported patterns:
        // 1) x is T t && t.Prop OP value
        // 2) x?.A?.B == c

        if (node is BinaryExpressionSyntax andExpr && andExpr.IsKind(SyntaxKind.LogicalAndExpression))
        {
            RegisterCodeFix(
                context,
                diagnostic,
                title: "Use property pattern",
                equivalenceKey: "UsePropertyPattern",
                createChangedDocument: c => ApplyAndPatternFixAsync(context.Document, andExpr, c));

            return Task.CompletedTask;
        }

        if (node is BinaryExpressionSyntax equalsExpr && equalsExpr.IsKind(SyntaxKind.EqualsExpression))
        {
            RegisterCodeFix(
                context,
                diagnostic,
                title: "Use nested property pattern",
                equivalenceKey: "UseNestedPropertyPattern",
                createChangedDocument: c => ApplyNullConditionalFixAsync(context.Document, equalsExpr, c));
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyAndPatternFixAsync(Document document, BinaryExpressionSyntax andExpr, CancellationToken ct)
    {
        // Rewrite:
        //   x is T t && t.Prop OP value
        // to:
        //   x is { Prop: OP value }

        if (andExpr.Left is not IsPatternExpressionSyntax isPattern)
            return document;

        if (andExpr.Right is not BinaryExpressionSyntax rightBinary)
            return document;

        if (rightBinary.Left is not MemberAccessExpressionSyntax memberAccess)
            return document;

        var propName = memberAccess.Name.Identifier.ValueText;

        var subpattern = SyntaxFactory.Subpattern(
            SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(propName)),
            SyntaxFactory.RelationalPattern(
                SyntaxFactory.Token(GetRelationalToken(rightBinary.Kind())),
                rightBinary.Right));

        // For == / != we use a constant pattern instead of relational.
        if (rightBinary.IsKind(SyntaxKind.EqualsExpression) || rightBinary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            var constant = SyntaxFactory.ConstantPattern(rightBinary.Right);
            subpattern = SyntaxFactory.Subpattern(
                SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(propName)),
                constant);

            if (rightBinary.IsKind(SyntaxKind.NotEqualsExpression))
            {
                subpattern = SyntaxFactory.Subpattern(
                    SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(propName)),
                    SyntaxFactory.UnaryPattern(SyntaxFactory.Token(SyntaxKind.NotKeyword), constant));
            }
        }

        var propertyPattern = SyntaxFactory.PropertyPatternClause(
            SyntaxFactory.SeparatedList(new[] { subpattern }));

        var recursivePattern = SyntaxFactory.RecursivePattern(
            type: null,
            positionalPatternClause: null,
            propertyPatternClause: propertyPattern,
            designation: null);

        var newIsPattern = isPattern.WithPattern(recursivePattern);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(andExpr, newIsPattern.WithAdditionalAnnotations(Formatter.Annotation));
        return editor.GetChangedDocument();
    }

    private static async Task<Document> ApplyNullConditionalFixAsync(Document document, BinaryExpressionSyntax equalsExpr, CancellationToken ct)
    {
        // Rewrite:
        //   x?.A?.B == c
        // to:
        //   x is { A: { B: c } }

        if (equalsExpr.Left is not ConditionalAccessExpressionSyntax)
            return document;

        if (!TryGetNullConditionalChain(equalsExpr.Left, out var rootExpr, out var members))
            return document;

        // Build nested property pattern from inside out.
        PatternSyntax current = SyntaxFactory.ConstantPattern(equalsExpr.Right);
        for (var i = members.Length - 1; i >= 0; i--)
        {
            var sub = SyntaxFactory.Subpattern(
                SyntaxFactory.NameColon(SyntaxFactory.IdentifierName(members[i])),
                current);

            var clause = SyntaxFactory.PropertyPatternClause(SyntaxFactory.SeparatedList(new[] { sub }));
            current = SyntaxFactory.RecursivePattern(null, null, clause, null);
        }

        var isPattern = SyntaxFactory.IsPatternExpression(rootExpr, current);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(equalsExpr, isPattern.WithAdditionalAnnotations(Formatter.Annotation));
        return editor.GetChangedDocument();
    }

    private static bool TryGetNullConditionalChain(ExpressionSyntax expr, out ExpressionSyntax root, out ImmutableArray<string> members)
    {
        var builder = ImmutableArray.CreateBuilder<string>();

        ExpressionSyntax? current = expr;
        while (current is ConditionalAccessExpressionSyntax ca)
        {
            if (ca.WhenNotNull is not MemberBindingExpressionSyntax mb)
            {
                root = expr;
                members = default;
                return false;
            }

            builder.Insert(0, mb.Name.Identifier.ValueText);
            current = ca.Expression;
        }

        root = current ?? expr;
        members = builder.ToImmutable();
        return members.Length > 0;
    }

    private static SyntaxKind GetRelationalToken(SyntaxKind binaryKind)
    {
        return binaryKind switch
        {
            SyntaxKind.GreaterThanExpression => SyntaxKind.GreaterThanToken,
            SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken,
            SyntaxKind.LessThanExpression => SyntaxKind.LessThanToken,
            SyntaxKind.LessThanOrEqualExpression => SyntaxKind.LessThanEqualsToken,
            _ => SyntaxKind.EqualsEqualsToken
        };
    }
}
