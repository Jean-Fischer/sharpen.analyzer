// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

/// <summary>
/// Polyfill required for init-only setters when compiling against frameworks that don't provide it.
/// </summary>
internal sealed class IsExternalInit
{
}
