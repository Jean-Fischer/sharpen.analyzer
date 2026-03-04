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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTargetTypedNewCodeFixProvider))]
[Shared]
public sealed class UseTargetTypedNewCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseTargetTypedNewRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp9OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var objectCreation = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

        if (objectCreation == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use target-typed new",
                createChangedDocument: c => ApplyFixAsync(context.Document, objectCreation, c),
                equivalenceKey: "UseTargetTypedNew"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp9OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    private static async Task<Document> ApplyFixAsync(Document document, ObjectCreationExpressionSyntax objectCreation, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root == null)
            return document;

        // new T(args) => new(args)
        // new T() { ... } => new() { ... }
        var argumentList = objectCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var newExpression = SyntaxFactory.ImplicitObjectCreationExpression(
                SyntaxFactory.Token(SyntaxKind.NewKeyword),
                argumentList,
                objectCreation.Initializer)
            .WithTriviaFrom(objectCreation);

        var newRoot = root.ReplaceNode(objectCreation, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
