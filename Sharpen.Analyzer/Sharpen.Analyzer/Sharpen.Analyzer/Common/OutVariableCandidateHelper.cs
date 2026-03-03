using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Common;

internal static class OutVariableCandidateHelper
{
    public static bool IsCandidate(SemanticModel semanticModel, ArgumentSyntax outArgument, bool outArgumentCanBeDiscarded)
    {
        if (outArgument.Expression is not IdentifierNameSyntax outArgumentIdentifier)
        {
            return false;
        }

        var enclosingStatement = outArgument.AncestorsAndSelf().OfType<StatementSyntax>().LastOrDefault();
        if (enclosingStatement == null)
        {
            return false;
        }

        var outVariableName = outArgumentIdentifier.Identifier.ValueText;

        // 1. The out argument must be a local variable.
        var variableDeclarator = GetLocalVariableDeclaratorForOutArgument(enclosingStatement, outVariableName);
        if (variableDeclarator == null)
        {
            return false;
        }

        // 2. If the local variable is initialized within the declaration, it means that it is used.
        if (variableDeclarator.Initializer != null)
        {
            return false;
        }

        // 3. The local variable must not be used before it is passed as an out argument.
        //    Also, if it is a requirement that the out argument can be discarded, it must not be used
        //    anywhere in code after the out argument.
        var localVariableSymbol = semanticModel.GetSymbolInfo(outArgumentIdentifier).Symbol;
        if (localVariableSymbol == null)
        {
            return false;
        }

        var localVariableTextSpan = variableDeclarator.Identifier.Span;
        var outArgumentTextSpan = outArgumentIdentifier.Span;

        var usagesOfTheLocalVariable = enclosingStatement
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Where(identifier =>
                identifier.Identifier.ValueText == outVariableName &&
                identifier.Span != localVariableTextSpan &&
                identifier != outArgumentIdentifier &&
                SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(identifier).Symbol, localVariableSymbol))
            .ToList();

        var numberOfUsagesBeforeOutArgument = usagesOfTheLocalVariable.Count(identifier =>
        {
            var span = identifier.Identifier.Span;
            return span.Start >= localVariableTextSpan.Start && span.End <= outArgumentTextSpan.Start;
        });

        var numberOfUsagesAfterOutArgument = usagesOfTheLocalVariable.Count(identifier => identifier.Identifier.Span.Start >= outArgumentTextSpan.End);

        var localVariableCouldBecomeOutVariableOrDiscarded =
            numberOfUsagesBeforeOutArgument == 0 &&
            (
                outArgumentCanBeDiscarded && numberOfUsagesAfterOutArgument == 0 ||
                !outArgumentCanBeDiscarded && numberOfUsagesAfterOutArgument > 0
            );

        if (!localVariableCouldBecomeOutVariableOrDiscarded)
        {
            return false;
        }

        // Scope validation is intentionally simplified for the initial migration.
        // If we ever see false positives/negatives, port the upstream scope logic.
        return true;
    }

    private static VariableDeclaratorSyntax? GetLocalVariableDeclaratorForOutArgument(StatementSyntax enclosingStatement, string outVariableName)
    {
        // Find the closest local declaration statement in the same statement list.
        // This is a simplified port of upstream logic.
        return enclosingStatement
            .DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(s => s.Declaration.Variables)
            .FirstOrDefault(v => v.Identifier.ValueText == outVariableName);
    }
}
