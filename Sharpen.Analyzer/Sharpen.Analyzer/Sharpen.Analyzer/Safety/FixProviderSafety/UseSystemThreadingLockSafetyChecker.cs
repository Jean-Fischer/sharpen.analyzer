using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class UseSystemThreadingLockSafetyChecker : IFixProviderSafetyChecker
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

        // Diagnostic is reported on the field variable identifier.
        var variable = node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        if (variable is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "field-variable-not-found");

        if (semanticModel.GetDeclaredSymbol(variable, cancellationToken) is not IFieldSymbol fieldSymbol)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "field-symbol-not-found");

        // Ensure System.Threading.Lock is available.
        var lockType = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.Lock");
        if (lockType is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "lock-type-missing");

        // Ensure no Monitor.* usage in the file (conservative).
        if (ContainsMonitorUsage(root, semanticModel, cancellationToken))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "monitor-usage-present");

        // Ensure the field is used only as lock target.
        if (!IsUsedOnlyInLockStatements(root, semanticModel, fieldSymbol, cancellationToken))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "field-not-dedicated-lock");

        return FixProviderSafetyResult.Safe();
    }

    private static bool ContainsMonitorUsage(SyntaxNode root, SemanticModel semanticModel, CancellationToken ct)
    {
        foreach (var memberAccess in root.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            if (memberAccess.Expression is not IdentifierNameSyntax { Identifier.ValueText: "Monitor" })
                continue;

            var symbol = semanticModel.GetSymbolInfo(memberAccess.Expression, ct).Symbol;
            if (symbol is INamedTypeSymbol type && type.ToDisplayString() == "System.Threading.Monitor")
                return true;
        }

        return false;
    }

    private static bool IsUsedOnlyInLockStatements(
        SyntaxNode root,
        SemanticModel semanticModel,
        IFieldSymbol fieldSymbol,
        CancellationToken ct)
    {
        var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        var anyUsage = false;

        foreach (var identifier in identifiers)
        {
            if (identifier.Identifier.ValueText != fieldSymbol.Name)
                continue;

            var symbol = semanticModel.GetSymbolInfo(identifier, ct).Symbol;
            if (!SymbolEqualityComparer.Default.Equals(symbol, fieldSymbol))
                continue;

            anyUsage = true;

            if (identifier.Parent is not LockStatementSyntax lockStatement)
                return false;

            if (!ReferenceEquals(lockStatement.Expression, identifier))
                return false;
        }

        return anyUsage;
    }
}