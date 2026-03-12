using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNullConditionalAssignmentCodeFixProvider))]
[Shared]
public sealed class UseNullConditionalAssignmentCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseNullConditionalAssignmentRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var ifStatement = node.FirstAncestorOrSelf<IfStatementSyntax>() ?? node as IfStatementSyntax;
        if (ifStatement is null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            checker: new NullConditionalAssignmentSafetyChecker(),
            syntaxTree: root.SyntaxTree,
            semanticModel: semanticModel,
            diagnostic: diagnostic,
            matchSucceeded: true,
            cancellationToken: context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        RegisterCodeFix(
            context: context,
            diagnostic: diagnostic,
            title: "Use null-conditional assignment",
            equivalenceKey: nameof(UseNullConditionalAssignmentCodeFixProvider),
            createChangedDocument: ct => ApplyAsync(context.Document, ifStatement, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, IfStatementSyntax ifStatement, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Re-find node in current root.
        var currentIf = root.FindNode(ifStatement.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<IfStatementSyntax>();
        if (currentIf is null)
            return document;

        if (!TryGetNullCheckedExpression(currentIf.Condition, out var checkedExpression))
            return document;

        if (!TryGetSingleStatement(currentIf.Statement, out var singleStatement))
            return document;

        if (singleStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
            return document;

        if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return document;

        if (assignment.Left is not MemberAccessExpressionSyntax memberAccess)
            return document;

        // Build: x?.Member = rhs;
        //
        // Roslyn represents this as a SimpleAssignmentExpression whose LHS is a ConditionalAccessExpression.
        // (Not as a ConditionalAccessExpression whose WhenNotNull is an assignment.)
        var conditionalAccess = SyntaxFactory.ConditionalAccessExpression(
            expression: StripParentheses(checkedExpression).WithTriviaFrom(memberAccess.Expression),
            whenNotNull: SyntaxFactory.MemberBindingExpression(memberAccess.Name));

        var newAssignment = SyntaxFactory.AssignmentExpression(
            kind: SyntaxKind.SimpleAssignmentExpression,
            left: conditionalAccess,
            operatorToken: assignment.OperatorToken,
            right: assignment.Right);

        var newStatement = SyntaxFactory.ExpressionStatement(newAssignment)
            .WithLeadingTrivia(currentIf.GetLeadingTrivia())
            .WithTrailingTrivia(currentIf.GetTrailingTrivia());

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(currentIf, newStatement);

        return editor.GetChangedDocument();
    }

    private static bool TryGetNullCheckedExpression(ExpressionSyntax condition, out ExpressionSyntax checkedExpression)
    {
        checkedExpression = null!;

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

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }
}
