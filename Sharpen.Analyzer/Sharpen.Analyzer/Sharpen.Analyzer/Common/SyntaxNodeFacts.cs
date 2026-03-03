using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Sharpen.Analyzer.Common;

public static class SyntaxNodeFacts
{
    public static bool AreEquivalent(SyntaxNode? left, SyntaxNode? right)
    {
        if (left is null || right is null)
            return false;

        // Legacy Sharpen used a custom equivalence check. For the analyzer's purposes,
        // Roslyn's built-in equivalence is sufficient and stable.
        return SyntaxFactory.AreEquivalent(left, right);
    }
}
