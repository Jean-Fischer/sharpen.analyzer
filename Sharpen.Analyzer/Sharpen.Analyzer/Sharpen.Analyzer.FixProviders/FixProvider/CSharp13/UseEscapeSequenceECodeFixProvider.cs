using System.Collections.Immutable;
using System.Composition;
using System.Text;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseEscapeSequenceECodeFixProvider))]
[Shared]
public sealed class UseEscapeSequenceECodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.UseEscapeSequenceERule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var literal =
            root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) as LiteralExpressionSyntax;
        if (literal is null)
            return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new UseEscapeSequenceESafetyChecker(),
            root.SyntaxTree,
            semanticModel,
            diagnostic,
            true,
            context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        RegisterCodeFix(
            context,
            diagnostic,
            "Use \\e escape sequence",
            nameof(UseEscapeSequenceECodeFixProvider),
            ct => ApplyAsync(context.Document, literal, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, LiteralExpressionSyntax literal,
        CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        var tokenText = literal.Token.Text;

        // Replace all \u001b occurrences.
        tokenText = tokenText.Replace("\\u001b", "\\e").Replace("\\u001B", "\\e");

        // Replace only unambiguous \x1b occurrences.
        tokenText = ReplaceUnambiguousX1B(tokenText);

        var newLiteral = literal.WithToken(SyntaxFactory.ParseToken(tokenText)).WithTriviaFrom(literal);

        editor.ReplaceNode(literal, newLiteral);

        return editor.GetChangedDocument();
    }

    private static string ReplaceUnambiguousX1B(string tokenText)
    {
        // Scan and replace occurrences of \x1b / \x1B where the next char is not a hex digit.
        // We build a new string to avoid overlapping replacements.
        var builder = new StringBuilder(tokenText.Length);

        for (var i = 0; i < tokenText.Length; i++)
        {
            if (i <= tokenText.Length - 4 &&
                tokenText[i] == '\\' &&
                (tokenText[i + 1] == 'x' || tokenText[i + 1] == 'X') &&
                tokenText[i + 2] == '1' &&
                (tokenText[i + 3] == 'b' || tokenText[i + 3] == 'B'))
            {
                var nextIndex = i + 4;
                if (nextIndex >= tokenText.Length || !IsHexDigit(tokenText[nextIndex]))
                {
                    builder.Append("\\e");
                    i += 3;
                    continue;
                }
            }

            builder.Append(tokenText[i]);
        }

        return builder.ToString();
    }

    private static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') ||
               (c >= 'a' && c <= 'f') ||
               (c >= 'A' && c <= 'F');
    }
}