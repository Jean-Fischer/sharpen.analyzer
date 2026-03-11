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

namespace Sharpen.Analyzer.FixProvider.CSharp11;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRawStringLiteralCodeFixProvider)), Shared]
public sealed class UseRawStringLiteralCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp11Rules.UseRawStringLiteralRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken).ConfigureAwait(false);
        if (compilation == null || !CSharpLanguageVersion.IsCSharp11OrAbove(compilation))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is not LiteralExpressionSyntax literal)
            return;

        // Only offer fix for non-verbatim, non-raw, non-interpolated string literals.
        if (!literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;

        var tokenText = literal.Token.Text;
        if (tokenText.StartsWith("@\"", StringComparison.Ordinal))
            return;

        if (literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
            || literal.Token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken))
        {
            return;
        }

        if (literal.Token.ValueText is not string valueText)
            return;

        // Keep it conservative: if the value contains CR/LF, we still allow (raw strings support it),
        // but we avoid any complex escaping rules by using ValueText directly.
        var rawLiteralText = CreateRawStringLiteralText(valueText);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Convert to raw string literal",
                createChangedDocument: ct => ConvertAsync(context.Document, literal, rawLiteralText, ct),
                equivalenceKey: nameof(UseRawStringLiteralCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> ConvertAsync(Document document, LiteralExpressionSyntax literal, string rawLiteralText, CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        var newToken = SyntaxFactory.Token(
            leading: literal.Token.LeadingTrivia,
            kind: SyntaxKind.MultiLineRawStringLiteralToken,
            text: rawLiteralText,
            valueText: literal.Token.ValueText,
            trailing: literal.Token.TrailingTrivia);

        var newLiteral = literal.WithToken(newToken);
        editor.ReplaceNode(literal, newLiteral);
        return editor.GetChangedDocument();
    }

    private static string CreateRawStringLiteralText(string valueText)
    {
        // Choose delimiter length so that the content does not contain the delimiter sequence.
        // Start with 3 quotes and increase as needed.
        var delimiterLength = 3;
        while (valueText.Contains(new string('"', delimiterLength), StringComparison.Ordinal))
            delimiterLength++;

        var delimiter = new string('"', delimiterLength);

        // For simplicity, we emit a multi-line raw string literal form:
        // """
        // <content>
        // """
        // This works for both single-line and multi-line content.
        return delimiter + "\r\n" + valueText + "\r\n" + delimiter;
    }
}
