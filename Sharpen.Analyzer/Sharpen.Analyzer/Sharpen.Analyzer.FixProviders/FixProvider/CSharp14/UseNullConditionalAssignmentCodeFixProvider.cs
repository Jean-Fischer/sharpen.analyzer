using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.Analyzers.CSharp14;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNullConditionalAssignmentCodeFixProvider))]
[Shared]
public sealed class UseNullConditionalAssignmentCodeFixProvider
    : SafetyCheckedSharpenCodeFixProvider<IfStatementSyntax, NullConditionalAssignmentSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseNullConditionalAssignmentRule.Id);

    protected override IfStatementSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node.FirstAncestorOrSelf<IfStatementSyntax>() ?? node as IfStatementSyntax;
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        IfStatementSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Use null-conditional assignment",
            nameof(UseNullConditionalAssignmentCodeFixProvider),
            ct => ApplyAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyAsync(Document document, IfStatementSyntax ifStatement,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Re-find node in current root.
        var currentIf = root.FindNode(ifStatement.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<IfStatementSyntax>();
        if (currentIf is null)
            return document;

        if (!NullConditionalAssignmentPattern.TryMatch(
                currentIf,
                out var checkedExpression,
                out var memberAccess,
                out var assignment))
        {
            return document;
        }

        // Build: x?.Member = rhs;
        //
        // Roslyn represents this as a SimpleAssignmentExpression whose LHS is a ConditionalAccessExpression.
        // (Not as a ConditionalAccessExpression whose WhenNotNull is an assignment.)
        var conditionalAccess = SyntaxFactory.ConditionalAccessExpression(
            NullConditionalAssignmentPattern.StripParentheses(checkedExpression)
                .WithTriviaFrom(memberAccess.Expression),
            SyntaxFactory.MemberBindingExpression(memberAccess.Name));

        var newAssignment = SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            conditionalAccess,
            assignment.OperatorToken,
            assignment.Right);

        var newStatement = SyntaxFactory.ExpressionStatement(newAssignment)
            .WithLeadingTrivia(currentIf.GetLeadingTrivia())
            .WithTrailingTrivia(currentIf.GetTrailingTrivia());

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(currentIf, newStatement);

        return editor.GetChangedDocument();
    }
}
