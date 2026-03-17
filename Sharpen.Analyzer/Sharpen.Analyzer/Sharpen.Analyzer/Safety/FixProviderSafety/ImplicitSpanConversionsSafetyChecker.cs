using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class ImplicitSpanConversionsSafetyChecker : IFixProviderSafetyChecker
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

        if (node is not InvocationExpressionSyntax asSpanInvocation)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "node-not-invocation");

        if (asSpanInvocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.ValueText: "AsSpan" })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-as-span");

        if (asSpanInvocation.ArgumentList.Arguments.Count != 0)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "as-span-has-args");

        if (asSpanInvocation.Parent is not ArgumentSyntax argument)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-argument");

        if (argument.Parent is not ArgumentListSyntax argumentList)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-argument-list");

        if (argumentList.Parent is not InvocationExpressionSyntax outerInvocation)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "no-outer-invocation");

        // Ensure the AsSpan call is the BCL MemoryExtensions.AsSpan.
        var asSpanSymbol = semanticModel.GetSymbolInfo(asSpanInvocation, cancellationToken).Symbol as IMethodSymbol;
        if (asSpanSymbol is null || asSpanSymbol.Name != "AsSpan" ||
            asSpanSymbol.ContainingType?.ToDisplayString() != "System.MemoryExtensions")
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-bcl-as-span");

        // Ensure removing AsSpan does not change overload resolution.
        var beforeSymbol = semanticModel.GetSymbolInfo(outerInvocation, cancellationToken).Symbol as IMethodSymbol;
        if (beforeSymbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "outer-symbol-null");

        var receiverExpression = (asSpanInvocation.Expression as MemberAccessExpressionSyntax)!.Expression;
        var newArgument = argument.WithExpression(receiverExpression.WithTriviaFrom(argument.Expression));
        var newArgumentList = argumentList.ReplaceNode(argument, newArgument);
        var newOuterInvocation = outerInvocation.WithArgumentList(newArgumentList);

        var speculativeSymbol = semanticModel.GetSpeculativeSymbolInfo(
            outerInvocation.SpanStart,
            newOuterInvocation,
            SpeculativeBindingOption.BindAsExpression).Symbol as IMethodSymbol;

        if (speculativeSymbol is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "speculative-symbol-null");

        if (!SymbolEqualityComparer.Default.Equals(beforeSymbol, speculativeSymbol))
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "overload-changed");

        return FixProviderSafetyResult.Safe();
    }
}