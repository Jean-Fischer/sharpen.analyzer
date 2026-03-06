using System;
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
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp6;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNameofExpressionForThrowingArgumentExceptionsCodeFixProvider)), Shared]
public sealed class UseNameofExpressionForThrowingArgumentExceptionsCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseNameofExpressionForThrowingArgumentExceptionsRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var stringLiteral = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ExpressionSyntax;
        if (stringLiteral is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use nameof",
                createChangedDocument: c => UseNameofAsync(context.Document, stringLiteral, c),
                equivalenceKey: "Use nameof"),
            diagnostic);
    }

    private static async Task<Document> UseNameofAsync(Document document, ExpressionSyntax stringLiteralExpression, CancellationToken cancellationToken)
    {
        if (!CSharp6SyntaxHelpers.TryGetStringLiteralValue(stringLiteralExpression, out var paramName))
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var nameofExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("nameof"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(paramName)))))
            .WithTriviaFrom(stringLiteralExpression);

        editor.ReplaceNode(stringLiteralExpression, nameofExpression);

        return editor.GetChangedDocument();
    }
}
