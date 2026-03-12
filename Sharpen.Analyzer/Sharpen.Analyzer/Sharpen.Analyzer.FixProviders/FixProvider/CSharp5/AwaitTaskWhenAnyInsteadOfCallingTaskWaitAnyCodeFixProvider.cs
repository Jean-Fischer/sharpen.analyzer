using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp5;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyCodeFixProvider))]
[Shared]
public sealed class AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context) =>
        TaskWaitToWhenCodeFixHelper.RegisterAsync(
            context,
            title: "Use await Task.WhenAny",
            equivalenceKey: "UseAwaitTaskWhenAny",
            whenMethodName: "WhenAny",
            fixableDiagnosticIds: () => FixableDiagnosticIds);
}
