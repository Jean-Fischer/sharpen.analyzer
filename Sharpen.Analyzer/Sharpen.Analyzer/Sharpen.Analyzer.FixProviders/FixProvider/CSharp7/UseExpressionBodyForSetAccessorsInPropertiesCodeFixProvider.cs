using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp7;

[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(UseExpressionBodyForSetAccessorsInPropertiesCodeFixProvider))]
public sealed class
    UseExpressionBodyForSetAccessorsInPropertiesCodeFixProvider : ExpressionBodiedAccessorCodeFixProviderBase
{
    protected override string DiagnosticId => Rules.GeneralRules.UseExpressionBodyForSetAccessorsInPropertiesRule.Id;

    protected override string Title => "Use expression-bodied set accessor";

    protected override string EquivalenceKey => "UseExpressionBodyForSetAccessorsInProperties";

    protected override Task<Document> CreateChangedDocumentAsync(Document document, AccessorDeclarationSyntax accessor,
        CancellationToken ct)
    {
        return ExpressionBodiedAccessorCodeFixHelper.UseExpressionBodyForSetAccessorAsync<PropertyDeclarationSyntax>(
            document, accessor, ct);
    }
}