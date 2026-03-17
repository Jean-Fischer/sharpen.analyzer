using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class CSharp14Rules
{
    public const string Category = "Sharpen.CSharp14";

    public static readonly DiagnosticDescriptor UseFieldKeywordInPropertiesRule = new(
        "SHARPEN065",
        "Use field-backed property",
        "Use field-backed property (C# 14 'field' keyword)",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 introduces field-backed properties via the 'field' keyword. This rule suggests converting eligible manual backing-field properties."
    );

    public static readonly DiagnosticDescriptor UsePartialConstructorsRule = new(
        "SHARPEN071",
        "Consider partial constructors",
        "Consider using a partial constructor (C# 14) for generated initialization",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 introduces partial constructors. This rule provides informational guidance when a constructor delegates initialization to a partial method (common in source generation patterns)."
    );

    public static readonly DiagnosticDescriptor UsePartialEventsRule = new(
        "SHARPEN072",
        "Consider partial events",
        "Consider using a partial event (C# 14) for generated event accessors",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 introduces partial events. This rule provides informational guidance when event add/remove accessors delegate to partial methods (common in source generation patterns)."
    );

    public static readonly DiagnosticDescriptor UseNullConditionalAssignmentRule = new(
        "SHARPEN066",
        "Use null-conditional assignment",
        "Use null-conditional assignment",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 supports null-conditional assignment. This rule suggests rewriting simple guarded assignments to use ?.="
    );

    public static readonly DiagnosticDescriptor UseUnboundGenericTypeInNameofRule = new(
        "SHARPEN067",
        "Use unbound generic type in nameof",
        "Use unbound generic type in nameof",
        Category,
        DiagnosticSeverity.Info,
        true,
        "When using nameof on a constructed generic type, prefer the unbound generic form (e.g., nameof(Dictionary<,>))."
    );

    public static readonly DiagnosticDescriptor UseLambdaParameterModifiersWithoutTypesRule = new(
        "SHARPEN068",
        "Use lambda parameter modifiers without types",
        "Use lambda parameter modifiers without types",
        Category,
        DiagnosticSeverity.Info,
        true,
        "Remove redundant lambda parameter types when they are only present to allow modifiers."
    );

    public static readonly DiagnosticDescriptor UseImplicitSpanConversionsRule = new(
        "SHARPEN069",
        "Remove redundant span conversion",
        "Remove redundant span conversion",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 introduces additional implicit conversions to Span<T>/ReadOnlySpan<T>. This rule suggests removing redundant explicit conversions such as AsSpan() when they add no semantic value."
    );

    public static readonly DiagnosticDescriptor UseExtensionBlocksRule = new(
        "SHARPEN070",
        "Use extension blocks",
        "Consider organizing extension methods into an extension block",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 introduces extension blocks. This rule suggests organizing multiple extension methods for the same receiver type into an extension block."
    );

    public static readonly DiagnosticDescriptor SuggestCompoundAssignmentOperatorsRule = new(
        "SHARPEN073",
        "Consider compound assignment operators",
        "Consider implementing a compound assignment operator for type '{0}'",
        Category,
        DiagnosticSeverity.Info,
        true,
        "C# 14 allows user-defined compound assignment operators. This rule suggests considering them for performance-sensitive types when '+=' is used and only a binary operator exists."
    );
}