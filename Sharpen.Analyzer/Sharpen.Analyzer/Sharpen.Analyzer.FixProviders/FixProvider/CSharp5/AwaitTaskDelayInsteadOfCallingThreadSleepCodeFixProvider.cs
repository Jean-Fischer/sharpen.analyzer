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

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        return ThreadSleepToTaskDelayCodeFixHelper.RegisterAsync(
            context,
            "Use await Task.Delay",
            "UseAwaitTaskDelay");
    }
}