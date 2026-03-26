using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseNameofExpressionInDependencyPropertyDeclarationsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.GeneralRules.UseNameofExpressionInDependencyPropertyDeclarationsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (!invocation.ArgumentList.Arguments.Any()) return;

        // We only care about the first argument (property name).
        var firstArgExpression = invocation.ArgumentList.Arguments[0].Expression;

        if (CSharp6SyntaxHelpers.IsNameofExpression(firstArgExpression)) return;

        if (!CSharp6SyntaxHelpers.TryGetStringLiteralValue(firstArgExpression, out var propertyName)) return;

        // Semantic check: ensure this is DependencyProperty.Register/RegisterAttached.
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol) return;

        if (!IsDependencyPropertyRegistrationMethod(methodSymbol)) return;

        // Symbol check: ensure a matching CLR property exists on the containing type.
        var containingType = context.ContainingSymbol?.ContainingType;
        if (containingType is null) return;

        var hasMatchingProperty = containingType
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Any(p => p.Name == propertyName);

        if (!hasMatchingProperty) return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.GeneralRules.UseNameofExpressionInDependencyPropertyDeclarationsRule,
                firstArgExpression.GetLocation()));
    }

    private static bool IsDependencyPropertyRegistrationMethod(IMethodSymbol methodSymbol)
    {
        if (methodSymbol.Name is not ("Register" or "RegisterAttached")) return false;

        // Must be DependencyProperty.Register*.
        if (methodSymbol.ContainingType is null || methodSymbol.ContainingType.Name != "DependencyProperty")
            return false;

        // We don't reference WPF assemblies; just check namespace string.
        var ns = methodSymbol.ContainingType.ContainingNamespace?.ToDisplayString();
        return ns == "System.Windows";
    }
}