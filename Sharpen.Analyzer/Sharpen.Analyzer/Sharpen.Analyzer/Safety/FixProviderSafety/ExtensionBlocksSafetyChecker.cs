using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class ExtensionBlocksSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        // Conservative: only allow same-file, non-partial, no preprocessor directives.
        // The code fix will only rewrite within the same class declaration.

        var root = syntaxTree.GetRoot(cancellationToken);

        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>() ?? node as ClassDeclarationSyntax;
        if (classDeclaration is null)
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-containing-class",
                "No containing class declaration.");
        }

        if (!classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-static",
                "Containing type is not static.");
        }

        if (classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "partial-type",
                "Partial types are not supported.");
        }

        if (classDeclaration.DescendantTrivia().Any(t => t.IsDirective))
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "has-directives",
                "Preprocessor directives are not supported.");
        }

        // Ensure we can find at least 2 extension methods.
        var extensionMethods = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.ParameterList.Parameters.Any() && m.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
            .ToList();

        if (extensionMethods.Count < 2)
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-enough-methods",
                "Not enough extension methods.");
        }

        var dominantGroup = extensionMethods
            .GroupBy(m => m.ParameterList.Parameters[0].Type?.ToString() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (dominantGroup is null || dominantGroup.Count() < 2)
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-dominant-group",
                "No dominant extension method group.");
        }

        if (dominantGroup.Select(m => m.ParameterList.Parameters[0].Identifier.ValueText).Distinct().Count() != 1)
        {
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "receiver-name-mismatch",
                "Dominant extension methods must share the same receiver parameter name.");
        }

        _ = semanticModel;
        return FixProviderSafetyResult.Safe();
    }
}
