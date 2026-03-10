using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp10Rules
{
    public static readonly DiagnosticDescriptor UseFileScopedNamespaceRule = new DiagnosticDescriptor(
        id: "SHARPEN040",
        title: "Use file-scoped namespace",
        messageFormat: "Convert this namespace to a file-scoped namespace",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 10 file-scoped namespaces reduce indentation and boilerplate.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );

    public static readonly DiagnosticDescriptor UseGlobalUsingDirectiveRule = new DiagnosticDescriptor(
        id: "SHARPEN041",
        title: "Use global using directive",
        messageFormat: "Convert this using directive to a global using directive",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 10 global using directives reduce repetition across files.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );

    public static readonly DiagnosticDescriptor UseRecordStructRule = new DiagnosticDescriptor(
        id: "SHARPEN042",
        title: "Use record struct",
        messageFormat: "Convert this struct to a record struct",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 10 record structs provide value-based semantics with concise syntax for value objects.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );

    public static readonly DiagnosticDescriptor UseExtendedPropertyPatternRule = new DiagnosticDescriptor(
        id: "SHARPEN043",
        title: "Use extended property pattern",
        messageFormat: "Rewrite this expression using a property pattern",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 10 extended property patterns can simplify nested member access checks.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );

    public static readonly DiagnosticDescriptor UseInterpolatedStringRule = new DiagnosticDescriptor(
        id: "SHARPEN044",
        title: "Use interpolated string",
        messageFormat: "Rewrite this expression using an interpolated string",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Interpolated strings can improve readability compared to string.Format or concatenation.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );

    public static readonly DiagnosticDescriptor UseConstInterpolatedStringRule = new DiagnosticDescriptor(
        id: "SHARPEN045",
        title: "Use const interpolated string",
        messageFormat: "Rewrite this const string using a const interpolated string",
        category: "Sharpen.CSharp10",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 10 allows const interpolated strings when all holes are compile-time constants.",
        helpLinkUri: "https://github.com/sharpen-analyzer/sharpen.analyzer"
    );
}
