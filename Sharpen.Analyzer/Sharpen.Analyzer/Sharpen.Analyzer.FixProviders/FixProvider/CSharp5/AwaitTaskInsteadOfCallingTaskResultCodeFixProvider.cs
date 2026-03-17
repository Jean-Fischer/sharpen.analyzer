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

namespace Sharpen.Analyzer.FixProvider.CSharp5;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitTaskInsteadOfCallingTaskResultCodeFixProvider))]
[Shared]
public sealed class AwaitTaskInsteadOfCallingTaskResultCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskInsteadOfCallingTaskResultRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);

        // Diagnostic is reported on the 'Result' identifier.
        var identifier = node as IdentifierNameSyntax;
        if (identifier is null) return;

        if (identifier.Parent is not MemberAccessExpressionSyntax memberAccess) return;

        // Only support simple patterns where the member access is the full expression:
        //   var x = task.Result;
        //   return task.Result;
        //   SomeMethod(task.Result);
        // Not supported:
        //   task.Result + 1
        //   task.Result.ToString()
        if (memberAccess.Parent is not (EqualsValueClauseSyntax or ReturnStatementSyntax or ArgumentSyntax)) return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null) return;

        if (!AsyncModernizationHelpers.IsAwaitLegalAt(memberAccess)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use await",
                c => ApplyFixAsync(context.Document, memberAccess, semanticModel, c),
                "UseAwait"),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // task.Result -> await task
        var awaited = SyntaxFactory.AwaitExpression(memberAccess.Expression.WithoutTrivia())
            .WithLeadingTrivia(memberAccess.GetLeadingTrivia())
            .WithTrailingTrivia(memberAccess.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(memberAccess, awaited);

        // Ensure containing callable is async.
        newRoot = AsyncModernizationHelpers.MakeContainingCallableAsync(newRoot, memberAccess, semanticModel);

        return document.WithSyntaxRoot(newRoot);
    }
}