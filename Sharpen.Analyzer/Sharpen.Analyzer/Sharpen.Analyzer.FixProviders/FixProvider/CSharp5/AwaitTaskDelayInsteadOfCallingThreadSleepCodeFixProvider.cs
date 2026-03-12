using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp5;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitTaskDelayInsteadOfCallingThreadSleepCodeFixProvider))]
[Shared]
public sealed class AwaitTaskDelayInsteadOfCallingThreadSleepCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskDelayInsteadOfCallingThreadSleepRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context) =>
        ThreadSleepToTaskDelayCodeFixHelper.RegisterAsync(
            context,
            title: "Use await Task.Delay",
            equivalenceKey: "UseAwaitTaskDelay");
}
