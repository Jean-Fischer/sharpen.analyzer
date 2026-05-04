using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.CSharp10;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseInterpolatedStringCodeFixProvider))]
[Shared]
public sealed class UseInterpolatedStringCodeFixProvider
    : SafetyCheckedSharpenCodeFixProvider<ExpressionSyntax, StringInterpolationSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(
            CSharp10Rules.UseInterpolatedStringRule.Id,
            CSharp10Rules.UseConstInterpolatedStringRule.Id);

    protected override ExpressionSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is InvocationExpressionSyntax invocation)
            return invocation;

        var add = node.FirstAncestorOrSelf<BinaryExpressionSyntax>();
        return add?.IsKind(SyntaxKind.AddExpression) == true ? add : null;
    }

    protected override async Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        ExpressionSyntax targetNode)
    {
        if (!await IsCSharp10OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        if (targetNode is InvocationExpressionSyntax invocation)
        {
            RegisterStringFormatFix(context, diagnostic, invocation);
            return;
        }

        if (targetNode is BinaryExpressionSyntax add && add.IsKind(SyntaxKind.AddExpression))
            RegisterConcatenationFix(context, diagnostic, add);
    }

    private static async Task<bool> IsCSharp10OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp10OrAbove(compilation);
    }

    private static async Task<Document> FixStringFormatAsync(Document document, InvocationExpressionSyntax invocation,
        CancellationToken ct)
    {
        // Convert string.Format("Hello, {0}!", name) => $"Hello, {name}!"
        if (invocation.Expression is not MemberAccessExpressionSyntax ma)
            return document;

        if (ma.Expression is not IdentifierNameSyntax id || id.Identifier.ValueText != "string")
            return document;

        if (ma.Name.Identifier.ValueText != "Format")
            return document;

        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 1)
            return document;

        if (args[0].Expression is not LiteralExpressionSyntax lit || !lit.IsKind(SyntaxKind.StringLiteralExpression))
            return document;

        var format = lit.Token.ValueText;
        var replacements = args.Skip(1).Select(a => a.Expression).ToList();

        var interpolatedText = BuildInterpolatedStringFromFormat(format, replacements);
        if (interpolatedText == null)
            return document;

        var newExpr = SyntaxFactory.ParseExpression(interpolatedText)
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(invocation, newExpr);
        return editor.GetChangedDocument();
    }

    private static string? BuildInterpolatedStringFromFormat(string format, List<ExpressionSyntax> args)
    {
        // Very small subset parser: supports {0} and {0:000}.
        // Escaped braces {{ and }} are preserved.

        var sb = new StringBuilder();
        sb.Append("$\"");

        for (var i = 0; i < format.Length; i++)
        {
            var ch = format[i];
            switch (ch)
            {
                case '{':
                    if (TryAppendEscapedBrace(format, "{", ref i, sb))
                        continue;

                    if (!TryAppendInterpolation(format, args, ref i, sb))
                        return null;

                    continue;

                case '}':
                    if (TryAppendEscapedBrace(format, "}", ref i, sb))
                        continue;

                    return null;

                case '"':
                    sb.Append("\\\"");
                    continue;

                default:
                    sb.Append(ch);
                    continue;
            }
        }

        sb.Append("\"");
        return sb.ToString();
    }

    private static async Task<Document> FixConcatenationAsync(Document document, BinaryExpressionSyntax add,
        CancellationToken ct)
    {
        // Convert "Hello, " + name + "!" => $"Hello, {name}!"
        // Only handles + chains.

        var interpolatedText = BuildInterpolatedStringFromConcatenation(add);
        if (interpolatedText == null)
            return document;

        var newExpr = SyntaxFactory.ParseExpression(interpolatedText)
            .WithTriviaFrom(add)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(add, newExpr);
        return editor.GetChangedDocument();
    }

    private static IEnumerable<ExpressionSyntax> FlattenAdd(ExpressionSyntax expr)
    {
        if (expr is BinaryExpressionSyntax bin && bin.IsKind(SyntaxKind.AddExpression))
        {
            foreach (var e in FlattenAdd(bin.Left))
                yield return e;
            foreach (var e in FlattenAdd(bin.Right))
                yield return e;
            yield break;
        }

        yield return expr;
    }

    private static void RegisterStringFormatFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        InvocationExpressionSyntax invocation)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Use interpolated string",
            "UseInterpolatedString_StringFormat",
            c => FixStringFormatAsync(context.Document, invocation, c));
    }

    private static void RegisterConcatenationFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        BinaryExpressionSyntax add)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Use interpolated string",
            "UseInterpolatedString_Concat",
            c => FixConcatenationAsync(context.Document, add, c));
    }

    private static bool TryAppendEscapedBrace(string format, string brace, ref int index, StringBuilder sb)
    {
        if (index + 1 >= format.Length || format[index + 1].ToString() != brace)
            return false;

        sb.Append(brace);
        index++;
        return true;
    }

    private static bool TryAppendInterpolation(
        string format,
        IReadOnlyList<ExpressionSyntax> args,
        ref int index,
        StringBuilder sb)
    {
        var end = format.IndexOf('}', index + 1);
        if (end < 0)
            return false;

        var inside = format.Substring(index + 1, end - index - 1);
        var parts = inside.Split(new[] { ':' }, 2);
        if (!int.TryParse(parts[0].Trim(), out var argIndex) || argIndex < 0 || argIndex >= args.Count)
            return false;

        sb.Append("{");
        sb.Append(args[argIndex]);
        if (parts.Length == 2)
        {
            sb.Append(":");
            sb.Append(parts[1]);
        }

        sb.Append("}");
        index = end;
        return true;
    }

    private static string? BuildInterpolatedStringFromConcatenation(BinaryExpressionSyntax add)
    {
        var parts = FlattenAdd(add).ToList();
        if (parts.Count < 2)
            return null;

        var sb = new StringBuilder();
        sb.Append("$\"");

        foreach (var part in parts)
        {
            if (part is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            {
                sb.Append(lit.Token.ValueText.Replace("\"", "\\\""));
                continue;
            }

            sb.Append("{");
            sb.Append(part);
            sb.Append("}");
        }

        sb.Append("\"");
        return sb.ToString();
    }
}
