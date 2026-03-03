using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class Rules
{
    public static DiagnosticDescriptor UseVarKeywordRule = new DiagnosticDescriptor(
        id: "SHARPEN002",
        title: "Use var keyword in variable declaration with object creation",
        messageFormat: "Use 'var' instead of explicit type '{0}' for this declaration",
        category: "Sharpen.CSharp3",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using 'var' improves readability and reduces redundancy when the type is obvious from the right-hand side."
    );
    public static DiagnosticDescriptor AwaitEquivalentAsynchronousMethodRule = new DiagnosticDescriptor(
        id: "SHARPEN003",
        title: "Use async equivalent",
        messageFormat: "Use '{0}Async' instead of '{0}' and await it.",
        category: "Sharpen.AsyncAwait",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using async equivalents improves responsiveness and scalability."
    );
}