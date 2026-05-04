using System.Linq;
using Microsoft.CodeAnalysis;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Common;

internal static class EquivalentAsynchronousMethodSignatureComparer
{
    public static bool IsAsynchronousEquivalent(IMethodSymbol? potentialEquivalent, IMethodSymbol method)
    {
        if (potentialEquivalent == null)
            return false;

        if (potentialEquivalent.ReturnsVoid)
            return false;

        if (method.ReturnsVoid)
        {
            if (!EquivalentAsynchronousMethodMetadata.KnownAwaitableTypes.Any(awaitableType =>
                    awaitableType.IsVoidEquivalent &&
                    awaitableType.RepresentsType(potentialEquivalent.ReturnType)))
            {
                return false;
            }
        }
        else
        {
            if (potentialEquivalent.ReturnType is not INamedTypeSymbol potentialEquivalentReturnType)
                return false;

            if (potentialEquivalentReturnType.Arity != 1)
                return false;

            var returnedKnownAwaitableType = EquivalentAsynchronousMethodMetadata.KnownAwaitableTypes
                .FirstOrDefault(awaitableType =>
                    awaitableType.RepresentsType(potentialEquivalentReturnType.ConstructedFrom));
            if (returnedKnownAwaitableType == null)
                return false;

            if (returnedKnownAwaitableType.WrapsReturnType())
            {
                if (!SymbolEqualityComparer.Default.Equals(method.ReturnType, potentialEquivalentReturnType.TypeArguments[0]))
                    return false;
            }
            else
            {
                if (method.ReturnType is not INamedTypeSymbol { Arity: 1 } methodReturnType)
                    return false;

                if (!SymbolEqualityComparer.Default.Equals(methodReturnType.TypeArguments[0], potentialEquivalentReturnType.TypeArguments[0]))
                    return false;
            }
        }

        var numberOfParameters = method.Parameters.Length;
        if (!(potentialEquivalent.Parameters.Length == numberOfParameters ||
              potentialEquivalent.Parameters.Length == numberOfParameters + 1))
        {
            return false;
        }

        for (var i = 0; i < numberOfParameters; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(method.Parameters[i].Type, potentialEquivalent.Parameters[i].Type))
                return false;

            if (method.Parameters[i].Name != potentialEquivalent.Parameters[i].Name)
                return false;
        }

        if (potentialEquivalent.Parameters.Length == numberOfParameters + 1 &&
            !potentialEquivalent.Parameters[numberOfParameters].Type.FullNameIsEqualTo("System.Threading", "CancellationToken"))
        {
            return false;
        }

        return true;
    }
}
