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
using Sharpen.Analyzer.Extensions;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCollectionExpressionCodeFixProvider))]
[Shared]
public sealed class UseCollectionExpressionCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UseCollectionExpressionRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // We report on the array creation expression itself.
        if (node is not ExpressionSyntax expression)
            return;

        var semanticModel = await context.Document
            .GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new CollectionExpressionSafetyChecker(),
            root.SyntaxTree,
            semanticModel,
            diagnostic,
            true,
            context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use collection expression",
                c => UseCollectionExpressionAsync(context.Document, expression, c),
                "UseCollectionExpression"),
            diagnostic);
    }

    private static async Task<Document> UseCollectionExpressionAsync(
        Document document,
        ExpressionSyntax expression,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var replacement = TryCreateCollectionExpression(expression);
        if (replacement is null)
            return document;

        var newRoot = root.ReplaceNode(expression, replacement);
        return document.WithSyntaxRoot(newRoot);
    }

    private static CollectionExpressionSyntax? TryCreateCollectionExpression(ExpressionSyntax expression)
    {
        // new T[] { 1, 2, 3 }
        if (expression is ArrayCreationExpressionSyntax arrayCreation)
        {
            if (arrayCreation.Initializer is null)
                return null;

            return CreateFromInitializer(arrayCreation.Initializer)
                .WithLeadingTrivia(arrayCreation.GetLeadingTrivia())
                .WithTrailingTrivia(arrayCreation.GetTrailingTrivia());
        }

        // new[] { 1, 2, 3 }
        if (expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreation)
        {
            if (implicitArrayCreation.Initializer is null)
                return null;

            return CreateFromInitializer(implicitArrayCreation.Initializer)
                .WithLeadingTrivia(implicitArrayCreation.GetLeadingTrivia())
                .WithTrailingTrivia(implicitArrayCreation.GetTrailingTrivia());
        }

        return null;
    }

    private static CollectionExpressionSyntax CreateFromInitializer(InitializerExpressionSyntax initializer)
    {
        // Preserve trivia on each element expression.
        var elements = initializer.Expressions
            .Select(e => (CollectionElementSyntax)SyntaxFactory.ExpressionElement(e))
            .ToArray();

        return CSharp12SyntaxFactory.CollectionExpression(SyntaxFactory.SeparatedList(elements));
    }
}