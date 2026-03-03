using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Common;

internal static class AsyncModernizationHelpers
{
    public static bool IsWithinAsyncCallable(SyntaxNode node, SemanticModel semanticModel)
    {
        var localFunction = node.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (localFunction != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(localFunction) as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        // Parenthesized/simple lambdas
        var lambda = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (lambda != null)
        {
            var symbol = semanticModel.GetSymbolInfo(lambda).Symbol as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        // Anonymous delegates
        var anonymousMethod = node.FirstAncestorOrSelf<AnonymousMethodExpressionSyntax>();
        if (anonymousMethod != null)
        {
            var symbol = semanticModel.GetSymbolInfo(anonymousMethod).Symbol as IMethodSymbol;
            return symbol?.IsAsync == true;
        }

        return false;
    }

    public static bool CanMakeContainingCallableAsync(SyntaxNode node, SemanticModel semanticModel)
    {
        // For this change, we only support:
        // - already-async callables (we can insert await)
        // - non-async callables that can be safely marked async locally
        if (IsWithinAsyncCallable(node, semanticModel)) return true;

        var localFunction = node.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (localFunction != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(localFunction) as IMethodSymbol;
            if (symbol == null) return false;

            // Only allow local functions that already return Task/Task<T>/ValueTask/ValueTask<T>.
            // (We don't support rewriting return types in this change.)
            if (!ReturnsKnownAwaitable(symbol)) return false;

            return !symbol.IsAsync;
        }

        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method != null)
        {
            var symbol = semanticModel.GetDeclaredSymbol(method) as IMethodSymbol;
            if (symbol == null) return false;

            // Only allow methods that already return Task/Task<T>/ValueTask/ValueTask<T>.
            // (We don't support rewriting return types in this change.)
            if (!ReturnsKnownAwaitable(symbol)) return false;

            // Avoid interface implementations / overrides where signature changes are risky.
            if (symbol.IsOverride) return false;
            if (symbol.ExplicitInterfaceImplementations.Length > 0) return false;
            if (symbol.ContainingType?.AllInterfaces.Any(i =>
                    i.GetMembers(symbol.Name).OfType<IMethodSymbol>().Any(m =>
                        SymbolEqualityComparer.Default.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(m), symbol))) == true)
            {
                return false;
            }

            return !symbol.IsAsync;
        }

        // Lambdas/anonymous delegates: we can insert await only if they are already async.
        // (We don't support adding the async modifier to them in this change.)
        return false;
    }

    private static bool ReturnsKnownAwaitable(IMethodSymbol method)
    {
        if (method.ReturnType == null) return false;

        var returnType = method.ReturnType;
        if (returnType is INamedTypeSymbol named && named.IsGenericType)
        {
            returnType = named.ConstructedFrom;
        }

        var fullName = returnType.ToDisplayString();
        return fullName == "System.Threading.Tasks.Task" ||
               fullName == "System.Threading.Tasks.ValueTask";
    }

    public static bool IsAwaitLegalAt(SyntaxNode node)
    {
        // Conservative: disallow await inside lock/finally/catch.
        // (We can expand later.)
        if (node.FirstAncestorOrSelf<LockStatementSyntax>() != null) return false;
        if (node.FirstAncestorOrSelf<CatchClauseSyntax>() != null) return false;
        if (node.FirstAncestorOrSelf<FinallyClauseSyntax>() != null) return false;

        return true;
    }

    public static SyntaxNode MakeContainingCallableAsync(SyntaxNode root, SyntaxNode node, SemanticModel semanticModel)
    {
        // If already async, nothing to do.
        if (IsWithinAsyncCallable(node, semanticModel)) return root;

        var localFunction = node.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
        if (localFunction != null)
        {
            if (localFunction.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword))) return root;

            var newLocalFunction = localFunction.WithModifiers(localFunction.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));
            return root.ReplaceNode(localFunction, newLocalFunction);
        }

        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method != null)
        {
            if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword))) return root;

            var newMethod = method.WithModifiers(method.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword)));
            return root.ReplaceNode(method, newMethod);
        }

        return root;
    }
}
