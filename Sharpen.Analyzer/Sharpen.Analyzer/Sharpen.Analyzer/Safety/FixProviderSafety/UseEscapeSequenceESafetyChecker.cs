using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class UseEscapeSequenceESafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (diagnostic is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var literalNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        if (literalNode is not LiteralExpressionSyntax literal)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "literal-not-found");

        var tokenText = literal.Token.Text;

        // Always safe for \u001b (fixed-length escape).
        if (tokenText.Contains("\\u001b") || tokenText.Contains("\\u001B"))
            return FixProviderSafetyResult.Safe();

        // For \x1b, only safe when the escape is not part of a longer hex escape sequence.
        // In C#, \x consumes as many hex digits as possible, so `\x1b2` is ambiguous.
        if (ContainsUnambiguousX1B(tokenText))
            return FixProviderSafetyResult.Safe();

        return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-safe-escape");
    }

    private static bool ContainsUnambiguousX1B(string tokenText)
    {
        // Scan for occurrences of \x1b or \x1B and ensure the next character is not a hex digit.
        for (var i = 0; i < tokenText.Length - 3; i++)
        {
            if (tokenText[i] != '\\')
                continue;

            if (tokenText[i + 1] != 'x' && tokenText[i + 1] != 'X')
                continue;

            if (tokenText[i + 2] != '1')
                continue;

            var b = tokenText[i + 3];
            if (b != 'b' && b != 'B')
                continue;

            var nextIndex = i + 4;
            if (nextIndex >= tokenText.Length)
                return true;

            if (!IsHexDigit(tokenText[nextIndex]))
                return true;
        }

        return false;
    }

    private static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') ||
               (c >= 'a' && c <= 'f') ||
               (c >= 'A' && c <= 'F');
    }
}