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

namespace Sharpen.Analyzer.FixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyCodeFixProvider))]
[Shared]
public sealed class AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        if (node is not InvocationExpressionSyntax invocation) return;

        // Only offer fix when the invocation is used as a statement (return value not used).
        if (invocation.Parent is not ExpressionStatementSyntax) return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(invocation)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use await Task.WhenAny",
                createChangedDocument: c => ApplyFixAsync(context.Document, invocation, semanticModel, c),
                equivalenceKey: "UseAwaitTaskWhenAny"),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // Task.WaitAny(args) -> await Task.WhenAny(args)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        var whenAnyMemberAccess = memberAccess.WithName(SyntaxFactory.IdentifierName("WhenAny"));
        var whenAnyInvocation = invocation.WithExpression(whenAnyMemberAccess);

        var awaited = SyntaxFactory.AwaitExpression(whenAnyInvocation.WithoutTrivia())
            .WithLeadingTrivia(invocation.GetLeadingTrivia())
            .WithTrailingTrivia(invocation.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(invocation, awaited);
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, invocation, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}
