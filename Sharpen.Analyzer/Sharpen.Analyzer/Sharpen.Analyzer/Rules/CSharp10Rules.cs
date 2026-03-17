using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp10Rules
{
    public static readonly DiagnosticDescriptor UseFileScopedNamespaceRule = new(
        "SHARPEN040",
        "Use file-scoped namespace",
        "Convert this namespace to a file-scoped namespace",
        "Sharpen.CSharp10",
        DiagnosticSeverity.Info,
        true,
        "C# 10 file-scoped namespaces reduce indentation and boilerplate."
    );


    public static readonly DiagnosticDescriptor UseRecordStructRule = new(
        "SHARPEN042",
        "Use record struct",
        "Convert this struct to a record struct",
        "Sharpen.CSharp10",
        DiagnosticSeverity.Info,
        true,
        "C# 10 record structs provide value-based semantics with concise syntax for value objects."
    );

    public static readonly DiagnosticDescriptor UseExtendedPropertyPatternRule = new(
        "SHARPEN043",
        "Use extended property pattern",
        "Rewrite this expression using a property pattern",
        "Sharpen.CSharp10",
        DiagnosticSeverity.Info,
        true,
        "C# 10 extended property patterns can simplify nested member access checks."
    );

    public static readonly DiagnosticDescriptor UseInterpolatedStringRule = new(
        "SHARPEN044",
        "Use interpolated string",
        "Rewrite this expression using an interpolated string",
        "Sharpen.CSharp10",
        DiagnosticSeverity.Info,
        true,
        "Interpolated strings can improve readability compared to string.Format or concatenation."
    );

    public static readonly DiagnosticDescriptor UseConstInterpolatedStringRule = new(
        "SHARPEN045",
        "Use const interpolated string",
        "Rewrite this const string using a const interpolated string",
        "Sharpen.CSharp10",
        DiagnosticSeverity.Info,
        true,
        "C# 10 allows const interpolated strings when all holes are compile-time constants."
    );
}