using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixProvider))]
[Shared]
public sealed class ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override Task RegisterCodeFixesAsync(CodeFixContext context) =>
        AsyncEquivalentInvocationCodeFixHelper.RegisterAsyncEquivalentFixAsync(context);
}
