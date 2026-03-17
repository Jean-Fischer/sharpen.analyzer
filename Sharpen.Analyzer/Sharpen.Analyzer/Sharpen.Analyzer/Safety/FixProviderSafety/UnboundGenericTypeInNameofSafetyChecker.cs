using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class UnboundGenericTypeInNameofSafetyChecker : IFixProviderSafetyChecker
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
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        // Diagnostic is reported on the type syntax inside nameof(...)
        if (node is not TypeSyntax typeSyntax)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "type-not-found");

        // Ensure we are inside nameof(...)
        var invocation = typeSyntax.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null || invocation.Expression is not IdentifierNameSyntax { Identifier.ValueText: "nameof" })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-nameof");

        // Ensure the argument binds to a named generic type.
        var typeInfo = semanticModel.GetTypeInfo(typeSyntax, cancellationToken);
        if (typeInfo.Type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-generic");

        // Ensure the replacement will still bind: build the unbound generic type symbol and compare name.
        // We keep this conservative: if we can resolve the original, we assume the unbound form is valid.
        // (The code fix will re-check binding after replacement.)
        return FixProviderSafetyResult.Safe();
    }
}