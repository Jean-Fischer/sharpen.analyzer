using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Common;

internal static class EquivalentAsynchronousMethodMetadata
{
    public const string AsyncSuffix = "Async";

    public static ImmutableArray<KnownAwaitableTypeInfo> KnownAwaitableTypes { get; } =
    [
        new("Task", "System.Threading.Tasks", KnownAwaitableTypeInfo.ReturnTypeWrappingKind.WrapsReturnType, true),
        new("ValueTask", "System.Threading.Tasks", KnownAwaitableTypeInfo.ReturnTypeWrappingKind.WrapsReturnType, true),
        new("IAsyncEnumerable", "System.Collections.Generic",
            KnownAwaitableTypeInfo.ReturnTypeWrappingKind.WrapsReturnTypeTypeParameter, true)
    ];

    private static ImmutableArray<MethodToIgnore> KnownMethodsToIgnore { get; } =
    [
        new("DbSet", "Microsoft.EntityFrameworkCore", "Add"),
        new("DbSet", "Microsoft.EntityFrameworkCore", "AddRange")
    ];

    public static bool IsIgnoredMethod(IMethodSymbol method)
    {
        return KnownMethodsToIgnore.Any(methodToIgnore => methodToIgnore.RepresentsMethod(method));
    }
}

internal sealed class KnownAwaitableTypeInfo(
    string typeName,
    string typeNamespace,
    KnownAwaitableTypeInfo.ReturnTypeWrappingKind wrappingKind,
    bool isVoidEquivalent)
    : KnownTypeInfo(typeName, typeNamespace)
{
    public enum ReturnTypeWrappingKind
    {
        WrapsReturnType,
        WrapsReturnTypeTypeParameter
    }

    public bool IsVoidEquivalent { get; } = isVoidEquivalent;

    public bool WrapsReturnType()
    {
        return wrappingKind == ReturnTypeWrappingKind.WrapsReturnType;
    }
}

internal sealed class MethodToIgnore(string typeName, string typeNamespace, string methodName)
    : KnownTypeInfo(typeName, typeNamespace)
{
    private string MethodName { get; } = methodName;

    public bool RepresentsMethod(IMethodSymbol method)
    {
        return RepresentsType(method.ContainingType) && MethodName == method.Name;
    }
}
