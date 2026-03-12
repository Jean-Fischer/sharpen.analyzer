using System;
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
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp11;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseUtf8StringLiteralCodeFixProvider)), Shared]
public sealed class UseUtf8StringLiteralCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.CSharp11Rules.UseUtf8StringLiteralRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken).ConfigureAwait(false);
        if (compilation == null || !CSharpLanguageVersion.IsCSharp11OrAbove(compilation))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Only offer fix when the expression is assigned to a ReadOnlySpan<byte> (or compatible) local.
        if (node is ExpressionSyntax expr)
        {
            var localDecl = expr.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
            if (localDecl == null)
                return;

            if (localDecl.Declaration.Variables.Count != 1)
                return;

            var variable = localDecl.Declaration.Variables[0];
            if (variable.Initializer?.Value != expr)
                return;

            var typeInfo = semanticModel.GetTypeInfo(localDecl.Declaration.Type, context.CancellationToken).Type;
            if (!IsReadOnlySpanOfByte(typeInfo))
                return;

            if (!TryGetAsciiText(semanticModel, expr, context.CancellationToken, out var text))
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Replace with UTF-8 string literal",
                    createChangedDocument: ct => ReplaceAsync(context.Document, expr, text, ct),
                    equivalenceKey: nameof(UseUtf8StringLiteralCodeFixProvider)),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceAsync(Document document, ExpressionSyntax expr, string text, CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // "text"u8
        var literal = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(text));

        var u8 = SyntaxFactory.Token(SyntaxKind.Utf8StringLiteralToken);

        // Roslyn doesn't expose a dedicated syntax node for the u8 suffix; use ParseExpression.
        var replacement = SyntaxFactory.ParseExpression($"\"{EscapeForRegularString(text)}\"u8")
            .WithTriviaFrom(expr);

        editor.ReplaceNode(expr, replacement);
        return editor.GetChangedDocument();
    }

    private static bool IsReadOnlySpanOfByte(ITypeSymbol? type)
    {
        if (type is not INamedTypeSymbol named)
            return false;

        if (named.ContainingNamespace?.ToDisplayString() != "System")
            return false;

        if (named.Name != "ReadOnlySpan" || named.TypeArguments.Length != 1)
            return false;

        return named.TypeArguments[0].SpecialType == SpecialType.System_Byte;
    }

    private static bool TryGetAsciiText(SemanticModel semanticModel, ExpressionSyntax expr, CancellationToken ct, out string text)
    {
        text = string.Empty;

        if (expr is ArrayCreationExpressionSyntax arrayCreation)
        {
            if (arrayCreation.Initializer == null)
                return false;

            var bytes = arrayCreation.Initializer.Expressions
                .Select(e => semanticModel.GetConstantValue(e, ct))
                .Select(v => v.HasValue ? v.Value : null)
                .Select(v => v is byte b ? (byte?)b : v is int i && i is >= 0 and <= 255 ? (byte?)i : null)
                .ToArray();

            if (bytes.Any(b => b == null))
                return false;

            var arr = bytes.Select(b => b!.Value).ToArray();
            if (!IsAscii(arr))
                return false;

            text = Encoding.ASCII.GetString(arr);
            return true;
        }

        if (expr is InvocationExpressionSyntax invocation)
        {
            var constant = semanticModel.GetConstantValue(invocation.ArgumentList.Arguments[0].Expression, ct);
            if (!constant.HasValue || constant.Value is not string s)
                return false;

            var bytes = Encoding.UTF8.GetBytes(s);
            if (!IsAscii(bytes))
                return false;

            text = s;
            return true;
        }

        return false;
    }

    private static bool IsAscii(byte[] bytes)
    {
        foreach (var b in bytes)
        {
            if (b == 0x09 || b == 0x0A || b == 0x0D)
                continue;

            if (b < 0x20 || b > 0x7E)
                return false;
        }

        return true;
    }

    private static string EscapeForRegularString(string text)
    {
        return text
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }
}
