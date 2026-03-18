using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp8;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIndexFromTheEndAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseIndexFromTheEndRule);

    public override void Initialize(AnalysisContext context)
    {
        // Legacy analyzer was stubbed (no diagnostics). Keep it as a no-op for now.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
    }
}