using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp11Rules
{
    public const string Category = "Sharpen.CSharp11";

    public static readonly DiagnosticDescriptor UseRawStringLiteralRule = new(
        "SHARPEN046",
        "Use raw string literal",
        "Use a raw string literal for improved readability",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 11 raw string literals improve readability for multi-line or heavily-escaped strings."
    );

    public static readonly DiagnosticDescriptor UseRequiredMemberRule = new(
        "SHARPEN047",
        "Use required member",
        "Mark this property as required",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 11 required members ensure properties are initialized during object creation."
    );

    public static readonly DiagnosticDescriptor UseGenericMathRule = new(
        "SHARPEN048",
        "Use generic math constraints",
        "Consider adding a generic math constraint (e.g., 'where {0} : INumber<{0}>')",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 11 generic math interfaces (System.Numerics) enable numeric operators on generic type parameters."
    );

    public static readonly DiagnosticDescriptor UseListPatternRule = new(
        "SHARPEN049",
        "Use list pattern",
        "Use a list pattern to simplify this length/indexing check",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 11 list patterns can simplify common span/array indexing patterns."
    );

    public static readonly DiagnosticDescriptor UseUtf8StringLiteralRule = new(
        "SHARPEN050",
        "Use UTF-8 string literal",
        "Use a UTF-8 string literal (\"...\"u8) instead of a byte array",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 11 UTF-8 string literals provide a concise way to represent UTF-8 bytes."
    );
}