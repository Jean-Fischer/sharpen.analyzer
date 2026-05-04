using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.Common;

public abstract class SafetyCheckedSharpenCodeFixProvider<TNode, TSafetyChecker> : SharpenCodeFixProvider
    where TNode : SyntaxNode
    where TSafetyChecker : IFixProviderSafetyChecker, new()
{
    protected sealed override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        return SafetyCheckedCodeFixRegistration.RegisterAsync(
            context,
            root,
            diagnostic,
            TryGetTargetNode,
            RegisterSafetyCheckedCodeFixesAsync,
            CreateSafetyChecker());
    }

    protected virtual TSafetyChecker CreateSafetyChecker()
    {
        return new TSafetyChecker();
    }

    protected abstract TNode? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic);

    protected abstract Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        TNode targetNode);
}
