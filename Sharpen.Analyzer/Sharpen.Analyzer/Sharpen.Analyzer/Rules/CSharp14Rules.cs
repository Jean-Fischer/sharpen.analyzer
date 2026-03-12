using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp14Rules
{
    public const string Category = "Sharpen.CSharp14";

    public static readonly DiagnosticDescriptor UseFieldKeywordInPropertiesRule = new DiagnosticDescriptor(
        id: "SHARPEN065",
        title: "Use field-backed property",
        messageFormat: "Use field-backed property (C# 14 'field' keyword)",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 14 introduces field-backed properties via the 'field' keyword. This rule suggests converting eligible manual backing-field properties."
    );

    public static readonly DiagnosticDescriptor UseNullConditionalAssignmentRule = new DiagnosticDescriptor(
        id: "SHARPEN066",
        title: "Use null-conditional assignment",
        messageFormat: "Use null-conditional assignment",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 14 supports null-conditional assignment. This rule suggests rewriting simple guarded assignments to use ?.="
    );

    public static readonly DiagnosticDescriptor UseUnboundGenericTypeInNameofRule = new DiagnosticDescriptor(
        id: "SHARPEN067",
        title: "Use unbound generic type in nameof",
        messageFormat: "Use unbound generic type in nameof",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "When using nameof on a constructed generic type, prefer the unbound generic form (e.g., nameof(Dictionary<,>))."
    );
}
