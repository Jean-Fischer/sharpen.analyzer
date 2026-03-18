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

namespace Sharpen.Analyzer.FixProvider.CSharp6;

[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(UseNameofExpressionInDependencyPropertyDeclarationsCodeFixProvider))]
[Shared]
public sealed class UseNameofExpressionInDependencyPropertyDeclarationsCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.UseNameofExpressionInDependencyPropertyDeclarationsRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        if (!(root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is ExpressionSyntax stringLiteralExpression)) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use nameof",
                c => UseNameofAsync(context.Document, stringLiteralExpression, c),
                "Use nameof"),
            diagnostic);
    }

    private static async Task<Document> UseNameofAsync(Document document, ExpressionSyntax stringLiteralExpression,
        CancellationToken cancellationToken)
    {
        if (!CSharp6SyntaxHelpers.TryGetStringLiteralValue(stringLiteralExpression, out var propertyName))
            return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var nameofExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("nameof"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(propertyName)))))
            .WithTriviaFrom(stringLiteralExpression);

        editor.ReplaceNode(stringLiteralExpression, nameofExpression);

        return editor.GetChangedDocument();
    }
}