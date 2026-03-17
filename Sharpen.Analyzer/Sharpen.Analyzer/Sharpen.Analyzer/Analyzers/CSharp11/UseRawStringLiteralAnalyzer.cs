using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRawStringLiteralAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp11Rules.UseRawStringLiteralRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp11OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
        });
    }

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;

        // Raw string literals are already the target form.
        if (literal.Token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken)
            || literal.Token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken))
            return;

        // Skip interpolated strings (not a LiteralExpressionSyntax anyway, but keep defensive).
        if (literal.IsKind(SyntaxKind.InterpolatedStringExpression))
            return;

        if (literal.Token.ValueText is not string valueText)
            return;

        if (!ShouldSuggestRawString(literal, valueText))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp11Rules.UseRawStringLiteralRule, literal.GetLocation()));
    }

    private static bool ShouldSuggestRawString(LiteralExpressionSyntax literal, string valueText)
    {
        // Multi-line: verbatim string with actual newlines, or regular string containing \n escapes.
        if (valueText.IndexOf("\n", StringComparison.Ordinal) >= 0)
            return true;

        var tokenText = literal.Token.Text;
        if (tokenText.StartsWith("@\"", StringComparison.Ordinal) &&
            (tokenText.IndexOf("\r", StringComparison.Ordinal) >= 0 ||
             tokenText.IndexOf("\n", StringComparison.Ordinal) >= 0))
            return true;

        // Escape density heuristic: count common escape sequences in the token text.
        // This is intentionally simple and conservative.
        var escapeCount = CountOccurrences(tokenText, "\\\\")
                          + CountOccurrences(tokenText, "\\\"")
                          + CountOccurrences(tokenText, "\\n")
                          + CountOccurrences(tokenText, "\\t")
                          + CountOccurrences(tokenText, "\\r");

        return escapeCount >= 3;
    }

    private static int CountOccurrences(string text, string pattern)
    {
        if (pattern.Length == 0)
            return 0;

        var count = 0;
        var index = 0;
        while (true)
        {
            index = text.IndexOf(pattern, index, StringComparison.Ordinal);
            if (index < 0)
                return count;

            count++;
            index += pattern.Length;
        }
    }
}