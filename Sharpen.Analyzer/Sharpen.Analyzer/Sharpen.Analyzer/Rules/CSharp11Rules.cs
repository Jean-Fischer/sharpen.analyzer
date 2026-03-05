using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp11Rules
{
    public const string Category = "Sharpen.CSharp11";

    public static readonly DiagnosticDescriptor UseRawStringLiteralRule = new DiagnosticDescriptor(
        id: "SHARPEN046",
        title: "Use raw string literal",
        messageFormat: "Use a raw string literal for improved readability",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 11 raw string literals improve readability for multi-line or heavily-escaped strings."
    );

    public static readonly DiagnosticDescriptor UseRequiredMemberRule = new DiagnosticDescriptor(
        id: "SHARPEN047",
        title: "Use required member",
        messageFormat: "Mark this property as required",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 11 required members ensure properties are initialized during object creation."
    );

    public static readonly DiagnosticDescriptor UseGenericMathRule = new DiagnosticDescriptor(
        id: "SHARPEN048",
        title: "Use generic math constraints",
        messageFormat: "Consider adding a generic math constraint (e.g., 'where {0} : INumber<{0}>')",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 11 generic math interfaces (System.Numerics) enable numeric operators on generic type parameters."
    );

    public static readonly DiagnosticDescriptor UseListPatternRule = new DiagnosticDescriptor(
        id: "SHARPEN049",
        title: "Use list pattern",
        messageFormat: "Use a list pattern to simplify this length/indexing check",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 11 list patterns can simplify common span/array indexing patterns."
    );

    public static readonly DiagnosticDescriptor UseUtf8StringLiteralRule = new DiagnosticDescriptor(
        id: "SHARPEN050",
        title: "Use UTF-8 string literal",
        messageFormat: "Use a UTF-8 string literal (\"...\"u8) instead of a byte array",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 11 UTF-8 string literals provide a concise way to represent UTF-8 bytes."
    );
}
