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
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.CSharp12;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCollectionExpressionCodeFixProvider))]
[Shared]
public sealed class UseCollectionExpressionCodeFixProvider
    : SafetyCheckedSharpenCodeFixProvider<ExpressionSyntax, CollectionExpressionSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UseCollectionExpressionRule.Id);

    protected override ExpressionSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node as ExpressionSyntax;
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        ExpressionSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Use collection expression",
            "UseCollectionExpression",
            c => UseCollectionExpressionAsync(context.Document, targetNode, c));

        return Task.CompletedTask;
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
