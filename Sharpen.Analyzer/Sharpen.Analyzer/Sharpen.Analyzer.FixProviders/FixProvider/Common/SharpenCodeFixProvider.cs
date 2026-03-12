using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Sharpen.Analyzer.FixProvider.Common;

public abstract class SharpenCodeFixProvider : CodeFixProvider
{
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await ShouldRegisterFixesAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        await RegisterCodeFixesAsync(context, root, diagnostic).ConfigureAwait(false);
    }

    protected virtual Task<bool> ShouldRegisterFixesAsync(Document document, CancellationToken ct) => Task.FromResult(true);

    protected abstract Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic);

    protected static void RegisterCodeFix(
        CodeFixContext context,
        Diagnostic diagnostic,
        string title,
        string equivalenceKey,
        Func<CancellationToken, Task<Document>> createChangedDocument)
    {
        context.RegisterCodeFix(
            CodeAction.Create(
                title: title,
                createChangedDocument: createChangedDocument,
                equivalenceKey: equivalenceKey),
            diagnostic);
    }
}
