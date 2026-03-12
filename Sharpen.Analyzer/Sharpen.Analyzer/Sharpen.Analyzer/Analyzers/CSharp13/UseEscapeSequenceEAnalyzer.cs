using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseEscapeSequenceEAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.UseEscapeSequenceERule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
        context.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.CharacterLiteralExpression);
    }

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LiteralExpressionSyntax literal)
            return;

        // We intentionally do a token-text based check (not value-based) because we only want to
        // suggest when the source uses \u001b or \x1b specifically.
        var text = literal.Token.Text;

        if (text.Contains("\\u001b") || text.Contains("\\u001B") ||
            text.Contains("\\x1b") || text.Contains("\\x1B"))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                CSharp13Rules.UseEscapeSequenceERule,
                literal.GetLocation()));
        }
    }
}
