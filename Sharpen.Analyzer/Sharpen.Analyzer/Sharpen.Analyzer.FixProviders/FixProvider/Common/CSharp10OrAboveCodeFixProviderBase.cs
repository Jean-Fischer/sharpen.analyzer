using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Sharpen.Analyzer.FixProvider.Common;

[Shared]
public abstract class CSharp10OrAboveCodeFixProviderBase : CSharp10OrAboveSharpenCodeFixProvider
{
    protected abstract string DiagnosticId { get; }

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

    protected abstract string Title { get; }

    protected abstract string EquivalenceKey { get; }

    protected abstract Task<Document> CreateChangedDocumentAsync(Document document, SyntaxNode root, Diagnostic diagnostic, CancellationToken ct);

    protected sealed override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            title: Title,
            equivalenceKey: EquivalenceKey,
            createChangedDocument: ct => CreateChangedDocumentAsync(context.Document, root, diagnostic, ct));

        return Task.CompletedTask;
    }
}
