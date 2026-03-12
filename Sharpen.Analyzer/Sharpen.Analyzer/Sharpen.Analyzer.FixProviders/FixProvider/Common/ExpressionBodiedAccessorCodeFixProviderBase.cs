using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.FixProvider.Common;

[Shared]
public abstract class ExpressionBodiedAccessorCodeFixProviderBase : CodeFixProvider
{
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    protected abstract string DiagnosticId { get; }

    protected abstract string Title { get; }

    protected abstract string EquivalenceKey { get; }

    protected abstract Task<Document> CreateChangedDocumentAsync(Document document, AccessorDeclarationSyntax accessor, CancellationToken ct);

    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var accessor = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<AccessorDeclarationSyntax>()
            .FirstOrDefault();

        if (accessor == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: ct => CreateChangedDocumentAsync(context.Document, accessor, ct),
                equivalenceKey: EquivalenceKey),
            diagnostic);
    }
}
