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
}
