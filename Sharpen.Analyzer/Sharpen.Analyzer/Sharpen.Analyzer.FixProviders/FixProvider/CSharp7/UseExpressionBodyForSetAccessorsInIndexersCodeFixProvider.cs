using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp7;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider))]
public sealed class
    UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider : ExpressionBodiedAccessorCodeFixProviderBase
{
    protected override string DiagnosticId => Rules.GeneralRules.UseExpressionBodyForSetAccessorsInIndexersRule.Id;

    protected override string Title => "Use expression-bodied set accessor";

    protected override string EquivalenceKey => "UseExpressionBodyForSetAccessorsInIndexers";

    protected override Task<Document> CreateChangedDocumentAsync(Document document, AccessorDeclarationSyntax accessor,
        CancellationToken ct)
    {
        return ExpressionBodiedAccessorCodeFixHelper.UseExpressionBodyForSetAccessorAsync<IndexerDeclarationSyntax>(
            document, accessor, ct);
    }
}