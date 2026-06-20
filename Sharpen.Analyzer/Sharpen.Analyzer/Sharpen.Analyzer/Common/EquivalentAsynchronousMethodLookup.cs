using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Common;

internal static class EquivalentAsynchronousMethodLookup
{
    public static bool TypeContainsAsynchronousEquivalent(
        SemanticModel semanticModel,
        INamedTypeSymbol? type,
        IMethodSymbol? method,
        InvocationExpressionSyntax? invocation = null)
    {
        return ResolveAsynchronousEquivalent(semanticModel, type, method, invocation) is not null;
    }

    public static IMethodSymbol? ResolveAsynchronousEquivalent(
        SemanticModel semanticModel,
        INamedTypeSymbol? type,
        IMethodSymbol? method,
        InvocationExpressionSyntax? invocation = null)
    {
        if (type == null || method == null)
            return null;

        var asynchronousEquivalentMethodName = method.Name + EquivalentAsynchronousMethodMetadata.AsyncSuffix;
        var candidates =
            (invocation == null
                ? type.GetMembers(asynchronousEquivalentMethodName)
                : semanticModel.LookupSymbols(
                        invocation.Expression?.SpanStart ?? 0,
                        type,
                        asynchronousEquivalentMethodName,
                        includeReducedExtensionMethods: true))
            .OfType<IMethodSymbol>();

        return candidates.FirstOrDefault(candidate =>
            EquivalentAsynchronousMethodSignatureComparer.IsAsynchronousEquivalent(candidate, method));
    }
}
