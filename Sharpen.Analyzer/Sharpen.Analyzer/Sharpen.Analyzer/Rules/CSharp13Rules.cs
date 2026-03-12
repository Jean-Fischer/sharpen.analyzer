using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp13Rules
{
    public const string Category = "Sharpen.CSharp13";

    public static readonly DiagnosticDescriptor PreferParamsCollectionsRule = new DiagnosticDescriptor(
        id: "SHARPEN058",
        title: "Prefer params collections",
        messageFormat: "Prefer collection-based params",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 supports collection-based params. Prefer a collection-based params type for non-public APIs when safe."
    );

    public static readonly DiagnosticDescriptor UseFromEndIndexInObjectInitializersRule = new DiagnosticDescriptor(
        id: "SHARPEN059",
        title: "Use from-end index in object initializers",
        messageFormat: "Use from-end index (^)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 supports from-end indices in object/collection initializers."
    );

    public static readonly DiagnosticDescriptor UseEscapeSequenceERule = new DiagnosticDescriptor(
        id: "SHARPEN060",
        title: "Use \\e escape sequence",
        messageFormat: "Use \\e escape sequence",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 introduces the \\e escape sequence for the ESC character."
    );

    public static readonly DiagnosticDescriptor UseSystemThreadingLockRule = new DiagnosticDescriptor(
        id: "SHARPEN061",
        title: "Use System.Threading.Lock",
        messageFormat: "Use System.Threading.Lock",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 introduces System.Threading.Lock for dedicated synchronization objects."
    );

    public static readonly DiagnosticDescriptor PartialPropertiesIndexersRefactoringRule = new DiagnosticDescriptor(
        id: "SHARPEN062",
        title: "Partial properties/indexers refactoring",
        messageFormat: "Consider refactoring to partial property/indexer",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 supports partial properties/indexers. This rule suggests refactoring opportunities."
    );

    public static readonly DiagnosticDescriptor SuggestAllowsRefStructConstraintRule = new DiagnosticDescriptor(
        id: "SHARPEN063",
        title: "Suggest allows ref struct constraint",
        messageFormat: "Consider adding 'allows ref struct' constraint (requires review)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 13 adds 'allows ref struct' constraints. This is a library design decision and requires review."
    );

    public static readonly DiagnosticDescriptor SuggestOverloadResolutionPriorityRule = new DiagnosticDescriptor(
        id: "SHARPEN064",
        title: "Suggest OverloadResolutionPriorityAttribute",
        messageFormat: "Consider OverloadResolutionPriorityAttribute (requires review)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "OverloadResolutionPriorityAttribute can guide overload selection. This is a library design decision and requires review."
    );
}
