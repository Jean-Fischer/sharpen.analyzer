using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp13Rules
{
    public const string Category = "Sharpen.CSharp13";

    public static readonly DiagnosticDescriptor PreferParamsCollectionsRule = new(
        "SHARPEN058",
        "Prefer params collections",
        "Prefer collection-based params",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 supports collection-based params. Prefer a collection-based params type for non-public APIs when safe."
    );

    public static readonly DiagnosticDescriptor UseFromEndIndexInObjectInitializersRule = new(
        "SHARPEN059",
        "Use from-end index in object initializers",
        "Use from-end index (^)",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 supports from-end indices in object/collection initializers."
    );

    public static readonly DiagnosticDescriptor UseEscapeSequenceERule = new(
        "SHARPEN060",
        "Use \\e escape sequence",
        "Use \\e escape sequence",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 introduces the \\e escape sequence for the ESC character."
    );

    public static readonly DiagnosticDescriptor UseSystemThreadingLockRule = new(
        "SHARPEN061",
        "Use System.Threading.Lock",
        "Use System.Threading.Lock",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 introduces System.Threading.Lock for dedicated synchronization objects."
    );

    public static readonly DiagnosticDescriptor PartialPropertiesIndexersRefactoringRule = new(
        "SHARPEN062",
        "Partial properties/indexers refactoring",
        "Consider refactoring to partial property/indexer",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 supports partial properties/indexers. This rule suggests refactoring opportunities."
    );

    public static readonly DiagnosticDescriptor SuggestAllowsRefStructConstraintRule = new(
        "SHARPEN063",
        "Suggest allows ref struct constraint",
        "Consider adding 'allows ref struct' constraint (requires review)",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 13 adds 'allows ref struct' constraints. This is a library design decision and requires review."
    );

    public static readonly DiagnosticDescriptor SuggestOverloadResolutionPriorityRule = new(
        "SHARPEN064",
        "Suggest OverloadResolutionPriorityAttribute",
        "Consider OverloadResolutionPriorityAttribute (requires review)",
        Category,
        DiagnosticSeverity.Info,
        true,
        "OverloadResolutionPriorityAttribute can guide overload selection. This is a library design decision and requires review."
    );
}