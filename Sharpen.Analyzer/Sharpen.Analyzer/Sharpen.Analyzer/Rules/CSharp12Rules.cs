using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp12Rules
{
    public const string Category = "Sharpen.CSharp12";

    public static readonly DiagnosticDescriptor UsePrimaryConstructorRule = new DiagnosticDescriptor(
        id: "SHARPEN051",
        title: "Use primary constructor",
        messageFormat: "Convert this type to use a primary constructor",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 12 primary constructors can reduce boilerplate when a constructor only assigns parameters to members."
    );

    public static readonly DiagnosticDescriptor UseCollectionExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN052",
        title: "Use collection expression",
        messageFormat: "Use a collection expression ([...])",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 12 collection expressions provide a concise syntax for array and collection initialization."
    );

    public static readonly DiagnosticDescriptor UseDefaultLambdaParametersRule = new DiagnosticDescriptor(
        id: "SHARPEN053",
        title: "Use default lambda parameters",
        messageFormat: "Use default values in the lambda parameter list",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 12 allows default values in lambda parameter lists when parameters are explicitly typed."
    );

    public static readonly DiagnosticDescriptor UseInlineArrayRule = new DiagnosticDescriptor(
        id: "SHARPEN054",
        title: "Use InlineArray",
        messageFormat: "Use [InlineArray({0})] for this fixed-size buffer struct",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 12 InlineArray can replace fixed-size buffer-like structs with a dedicated attribute-based representation."
    );
}
