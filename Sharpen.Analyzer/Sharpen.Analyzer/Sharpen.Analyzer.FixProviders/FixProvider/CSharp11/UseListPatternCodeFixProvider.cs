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
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp11;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseListPatternCodeFixProvider))]
[Shared]
public sealed class UseListPatternCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp11Rules.UseListPatternRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics[0];
        var ifStatement = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<IfStatementSyntax>();
        if (ifStatement == null)
            return;

        // Only handle the simplest unambiguous pattern:
        // if (x.Length > 0) { ... }
        // if (0 < x.Length) { ... }
        // if (x.Length != 0) { ... }
        // if (0 != x.Length) { ... }
        if (!TryGetLengthTarget(ifStatement.Condition, out var targetExpression))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use list pattern",
                ct => ApplyFixAsync(context.Document, ifStatement, targetExpression, ct),
                "Use list pattern"),
            diagnostic);
    }

    private static bool TryGetLengthTarget(ExpressionSyntax condition, out ExpressionSyntax target)
    {
        target = null!;

        if (condition is not BinaryExpressionSyntax binary)
            return false;

        if (!binary.IsKind(SyntaxKind.GreaterThanExpression)
            && !binary.IsKind(SyntaxKind.NotEqualsExpression)
            && !binary.IsKind(SyntaxKind.LessThanExpression))
        {
            return false;
        }

        // x.Length > 0
        if (binary.Left is MemberAccessExpressionSyntax leftMember
            && leftMember.Name.Identifier.ValueText == "Length"
            && binary.Right is LiteralExpressionSyntax rightLiteral
            && rightLiteral.Token.ValueText == "0")
        {
            target = leftMember.Expression;
            return true;
        }

        // 0 < x.Length OR 0 != x.Length
        if (binary.Right is MemberAccessExpressionSyntax rightMember
            && rightMember.Name.Identifier.ValueText == "Length"
            && binary.Left is LiteralExpressionSyntax leftLiteral
            && leftLiteral.Token.ValueText == "0")
        {
            target = rightMember.Expression;
            return true;
        }

        return false;
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        IfStatementSyntax ifStatement,
        ExpressionSyntax targetExpression,
        CancellationToken cancellationToken)
    {
        _ = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Build: x is [_, ..]
        // Note: list patterns are C# 11 syntax. The Roslyn version referenced by this repo does not
        // expose the required syntax node factories (and cannot parse list patterns), so we cannot
        // implement a compiling code fix here without upgrading Roslyn.
        //
        // Keep the provider present (so the change can be completed later), but do not apply changes.
        return document;
    }
}