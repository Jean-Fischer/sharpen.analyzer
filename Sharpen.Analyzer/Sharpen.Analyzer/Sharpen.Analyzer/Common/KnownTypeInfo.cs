using Microsoft.CodeAnalysis;
using Sharpen.Engine.Extensions;

namespace Sharpen.Engine;

internal abstract class KnownTypeInfo
{
    protected KnownTypeInfo(string typeName, string typeNamespace)
    {
        TypeName = typeName;
        TypeNamespace = typeNamespace;
    }

    public string TypeName { get; }
    public string TypeNamespace { get; }

    public bool RepresentsType(ITypeSymbol type)
    {
        return type.FullNameIsEqualTo(TypeNamespace, TypeName);
    }
}