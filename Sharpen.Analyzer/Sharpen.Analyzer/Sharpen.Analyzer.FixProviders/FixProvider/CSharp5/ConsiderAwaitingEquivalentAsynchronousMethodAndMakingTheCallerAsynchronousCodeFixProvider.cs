using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.FixProvider.Common;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp,
    Name = nameof(ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixProvider))]
[Shared]
public sealed class
    ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule
            .Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        return AsyncEquivalentInvocationCodeFixHelper.RegisterAsyncEquivalentFixAsync(context);
    }
}