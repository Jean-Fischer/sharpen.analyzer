using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp6;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider))]
public sealed class
    UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider : ExpressionBodiedAccessorCodeFixProviderBase
{
    protected override string DiagnosticId => Rules.GeneralRules.UseExpressionBodyForGetAccessorsInIndexersRule.Id;

    protected override string Title => "Use expression-bodied get accessor";

    protected override string EquivalenceKey => "UseExpressionBodyForGetAccessorsInIndexers";

    protected override Task<Document> CreateChangedDocumentAsync(Document document, AccessorDeclarationSyntax accessor,
        CancellationToken ct)
    {
        return ExpressionBodiedAccessorCodeFixHelper.UseExpressionBodyForGetAccessorAsync<IndexerDeclarationSyntax>(
            document,
            accessor,
            ct,
            static accessorList => accessorList.Accessors.Count > 1);
    }
}