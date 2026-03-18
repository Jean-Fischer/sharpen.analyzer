using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitEquivalentAsynchronousMethodCodeFixProvider))]
[Shared]
public class AwaitEquivalentAsynchronousMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.AwaitEquivalentAsynchronousMethodRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.First();
        var invocation = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
        if (invocation is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use async equivalent",
                c => AsyncEquivalentInvocationCodeFixHelper.ApplyAsyncEquivalentAsync(context.Document, invocation, c),
                "UseAsyncEquivalent"),
            diagnostic);
    }
}