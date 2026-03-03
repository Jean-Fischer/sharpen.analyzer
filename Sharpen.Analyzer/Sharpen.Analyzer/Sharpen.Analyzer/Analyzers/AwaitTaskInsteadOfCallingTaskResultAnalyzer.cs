using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AwaitTaskInsteadOfCallingTaskResultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.AwaitTaskInsteadOfCallingTaskResultRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        if (memberAccess.Name.Identifier.ValueText != "Result") return;

        // Ensure this is Task<T>.Result (not some other Result property)
        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IPropertySymbol;
        if (symbol is null) return;

        if (symbol.ContainingType is not INamedTypeSymbol containingType) return;
        if (!containingType.IsGenericType) return;

        // In some contexts Roslyn returns the constructed type (Task<int>) rather than the open generic.
        // Compare against the open generic definition.
        var constructedFrom = containingType.ConstructedFrom;
        if (constructedFrom.ContainingNamespace?.ToDisplayString() != "System.Threading.Tasks") return;
        if (constructedFrom.Name != "Task") return;
        if (constructedFrom.TypeArguments.Length != 1) return;

        // Only report when we can make the containing callable async.
        if (!AsyncModernizationHelpers.CanMakeContainingCallableAsync(memberAccess, context.SemanticModel)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.Rules.AwaitTaskInsteadOfCallingTaskResultRule,
            memberAccess.Name.GetLocation()));
    }
}
