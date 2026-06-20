using System.Linq;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Common;

internal static class EquivalentAsynchronousMethodSignatureComparer
{
    public static bool IsAsynchronousEquivalent(IMethodSymbol? potentialEquivalent, IMethodSymbol method)
    {
        return potentialEquivalent is not null &&
               !potentialEquivalent.ReturnsVoid &&
               HasCompatibleReturnType(potentialEquivalent, method) &&
               HasCompatibleParameters(potentialEquivalent, method);
    }

    private static bool HasCompatibleReturnType(IMethodSymbol potentialEquivalent, IMethodSymbol method)
    {
        if (method.ReturnsVoid)
            return HasVoidEquivalentReturnType(potentialEquivalent.ReturnType);

        var awaitableReturnType = GetAwaitableReturnType(potentialEquivalent.ReturnType);
        if (awaitableReturnType == null)
            return false;

        var (potentialEquivalentReturnType, returnedKnownAwaitableType) = awaitableReturnType.Value;
        return returnedKnownAwaitableType.WrapsReturnType()
            ? SymbolEqualityComparer.Default.Equals(method.ReturnType, potentialEquivalentReturnType.TypeArguments[0])
            : MethodTypeArgumentMatches(potentialEquivalentReturnType, method.ReturnType);
    }

    private static bool HasVoidEquivalentReturnType(ITypeSymbol returnType)
    {
        return EquivalentAsynchronousMethodMetadata.KnownAwaitableTypes.Any(awaitableType =>
            awaitableType.IsVoidEquivalent &&
            awaitableType.RepresentsType(returnType));
    }

    private static (INamedTypeSymbol ReturnType, KnownAwaitableTypeInfo AwaitableType)? GetAwaitableReturnType(
        ITypeSymbol returnType)
    {
        if (returnType is not INamedTypeSymbol { Arity: 1 } namedReturnType)
            return null;

        var returnedKnownAwaitableType = EquivalentAsynchronousMethodMetadata.KnownAwaitableTypes
            .FirstOrDefault(awaitableType => awaitableType.RepresentsType(namedReturnType.ConstructedFrom));

        return returnedKnownAwaitableType == null ? null : (namedReturnType, returnedKnownAwaitableType);
    }

    private static bool MethodTypeArgumentMatches(
        INamedTypeSymbol potentialEquivalentReturnType,
        ITypeSymbol methodReturnType)
    {
        return methodReturnType is INamedTypeSymbol { Arity: 1 } namedMethodReturnType &&
               SymbolEqualityComparer.Default.Equals(
                   namedMethodReturnType.TypeArguments[0],
                   potentialEquivalentReturnType.TypeArguments[0]);
    }

    private static bool HasCompatibleParameters(IMethodSymbol potentialEquivalent, IMethodSymbol method)
    {
        var numberOfParameters = method.Parameters.Length;
        if (potentialEquivalent.Parameters.Length != numberOfParameters &&
            potentialEquivalent.Parameters.Length != numberOfParameters + 1)
        {
            return false;
        }

        return ParametersMatch(potentialEquivalent, method, numberOfParameters) &&
               HasSupportedTrailingParameter(potentialEquivalent, numberOfParameters);
    }

    private static bool ParametersMatch(IMethodSymbol potentialEquivalent, IMethodSymbol method, int numberOfParameters)
    {
        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, potentialEquivalent.Parameters[i].Type))
                return false;

            if (method.Parameters[i].Name != potentialEquivalent.Parameters[i].Name)
                return false;
        }

        return true;
    }

    private static bool HasSupportedTrailingParameter(IMethodSymbol potentialEquivalent, int numberOfParameters)
    {
        if (potentialEquivalent.Parameters.Length == numberOfParameters)
            return true;

        var trailingParameter = potentialEquivalent.Parameters[numberOfParameters];
        return trailingParameter.Type.FullNameIsEqualTo("System.Threading", "CancellationToken")
               && trailingParameter.IsOptional;
    }
}
