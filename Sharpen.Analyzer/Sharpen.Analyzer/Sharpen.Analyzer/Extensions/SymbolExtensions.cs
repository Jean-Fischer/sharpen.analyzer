using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Extensions;

internal static class SymbolExtensions
{
    public static IEnumerable<IMethodSymbol> GetImplementedInterfaceMethods(this IMethodSymbol? methodSymbol)
    {
        if (methodSymbol == null)
            return Enumerable.Empty<IMethodSymbol>();

        var methodContainingType = methodSymbol.ContainingType;
        if (methodContainingType == null)
            return methodSymbol.ExplicitInterfaceImplementations;

        var implicitInterfaceImplementations = methodContainingType
            .AllInterfaces
            // Beware of the fact that for explicit interface implementations
            // the methodSymbol.Name is not the same as interface name ;-)
            // Therefore, this will filter out only the implicit implementations
            // and at the end we will add the explicit interface implementations.
            .SelectMany(@interface => @interface.GetMembers(methodSymbol.Name).OfType<IMethodSymbol>())
            .Where(interfaceMethod =>
                SymbolEqualityComparer.Default.Equals(methodContainingType.FindImplementationForInterfaceMember(interfaceMethod), methodSymbol));

        return implicitInterfaceImplementations.Concat(methodSymbol.ExplicitInterfaceImplementations);
    }
}