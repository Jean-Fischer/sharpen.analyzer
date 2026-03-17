using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp12Rules
{
    public const string Category = "Sharpen.CSharp12";

    public static readonly DiagnosticDescriptor UsePrimaryConstructorRule = new(
        "SHARPEN051",
        "Use primary constructor",
        "Convert this type to use a primary constructor",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 12 primary constructors can reduce boilerplate when a constructor only assigns parameters to members."
    );

    public static readonly DiagnosticDescriptor UseCollectionExpressionRule = new(
        "SHARPEN052",
        "Use collection expression",
        "Use a collection expression ([...])",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 12 collection expressions provide a concise syntax for array and collection initialization."
    );

    public static readonly DiagnosticDescriptor UseDefaultLambdaParametersRule = new(
        "SHARPEN053",
        "Use default lambda parameters",
        "Use default values in the lambda parameter list",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 12 allows default values in lambda parameter lists when parameters are explicitly typed."
    );

    public static readonly DiagnosticDescriptor UseInlineArrayRule = new(
        "SHARPEN054",
        "Use InlineArray",
        "Use [InlineArray({0})] for this fixed-size buffer struct",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 12 InlineArray can replace fixed-size buffer-like structs with a dedicated attribute-based representation."
    );
}