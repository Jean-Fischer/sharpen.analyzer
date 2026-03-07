using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class Rules
{
    // C# 3
    public static readonly DiagnosticDescriptor UseVarKeywordRule = new DiagnosticDescriptor(
        id: "SHARPEN002",
        title: "Use var keyword in variable declaration with object creation",
        messageFormat: "Use 'var' instead of explicit type '{0}' for this declaration",
        category: "Sharpen.CSharp3",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using 'var' improves readability and reduces redundancy when the type is obvious from the right-hand side."
    );

    public static readonly DiagnosticDescriptor UseInitOnlySetterRule = new DiagnosticDescriptor(
        id: "SHARPEN035",
        title: "Use init-only setter",
        messageFormat: "Use 'init' instead of 'private set' for this auto-property",
        category: "Sharpen.CSharp9",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 9 introduces init-only setters to express immutability after initialization."
    );

    public static readonly DiagnosticDescriptor UseRecordTypeRule = new DiagnosticDescriptor(
        id: "SHARPEN036",
        title: "Use record type",
        messageFormat: "Convert this sealed data class to a record",
        category: "Sharpen.CSharp9",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 9 records provide concise syntax and value-based semantics for data-centric types."
    );

    public static readonly DiagnosticDescriptor UseTopLevelStatementsRule = new DiagnosticDescriptor(
        id: "SHARPEN037",
        title: "Use top-level statements",
        messageFormat: "Convert this entry point to top-level statements",
        category: "Sharpen.CSharp9",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 9 top-level statements can reduce boilerplate for simple programs."
    );

    public static readonly DiagnosticDescriptor UseCSharp9PatternMatchingRule = new DiagnosticDescriptor(
        id: "SHARPEN038",
        title: "Use C# 9 pattern matching",
        messageFormat: "Rewrite this expression using C# 9 pattern matching",
        category: "Sharpen.CSharp9",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 9 adds relational and logical patterns that can simplify boolean expressions."
    );

    public static readonly DiagnosticDescriptor UseTargetTypedNewRule = new DiagnosticDescriptor(
        id: "SHARPEN039",
        title: "Use target-typed new",
        messageFormat: "Use target-typed 'new'",
        category: "Sharpen.CSharp9",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 9 target-typed new expressions can reduce redundancy when the type is obvious from context."
    );

    public static readonly DiagnosticDescriptor ReplaceUsingStatementWithUsingDeclarationRule = new DiagnosticDescriptor(
        id: "SHARPEN025",
        title: "Replace using statement with using declaration",
        messageFormat: "Replace using statement with using declaration",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 8 allows using declarations which can reduce nesting and improve readability."
    );

    public static readonly DiagnosticDescriptor ReplaceSwitchStatementWithSwitchExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN026",
        title: "Replace switch statement with switch expression",
        messageFormat: "Replace switch statement with switch expression",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 8 switch expressions can reduce boilerplate and improve readability."
    );

    public static readonly DiagnosticDescriptor EnableNullableContextAndDeclareIdentifierAsNullableRule = new DiagnosticDescriptor(
        id: "SHARPEN033",
        title: "Enable nullable context and declare identifier as nullable",
        messageFormat: "Enable nullable context and declare identifier as nullable",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 8 introduces nullable reference types. Enabling nullable context and marking identifiers as nullable can improve null-safety."
    );

    public static readonly DiagnosticDescriptor UseIndexFromTheEndRule = new DiagnosticDescriptor(
        id: "SHARPEN034",
        title: "Use index from the end",
        messageFormat: "Use index from the end",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 8 introduces indices and ranges. This rule is currently a placeholder for the migrated legacy analyzer."
    );

    public static readonly DiagnosticDescriptor UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule = new DiagnosticDescriptor(
        id: "SHARPEN031",
        title: "Use ??= operator instead of assigning result of the ?? operator",
        messageFormat: "Use ??= operator instead of assigning result of the ?? operator",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 8 introduces the ??= operator which can simplify null-coalescing assignments."
    );

    public static readonly DiagnosticDescriptor ConsiderUsingNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule = new DiagnosticDescriptor(
        id: "SHARPEN032",
        title: "Consider using ??= operator instead of assigning result of the ?? operator",
        messageFormat: "Consider using ??= operator instead of assigning result of the ?? operator",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This assignment may be replaceable with ??=, but conversion could change semantics due to side effects."
    );

    public static readonly DiagnosticDescriptor ConsiderReplacingSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN027",
        title: "Consider replacing switch statement with switch expression",
        messageFormat: "Consider replacing switch statement with switch expression",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This switch statement may be convertible to a switch expression, but conversion could change semantics."
    );

    public static readonly DiagnosticDescriptor ReplaceSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN028",
        title: "Replace switch statement with switch expression",
        messageFormat: "Replace switch statement with switch expression",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This switch statement can be safely replaced with a switch expression."
    );

    public static readonly DiagnosticDescriptor ConsiderReplacingSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN029",
        title: "Consider replacing switch statement with switch expression",
        messageFormat: "Consider replacing switch statement with switch expression",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This switch statement may be convertible to a switch expression, but conversion could change semantics."
    );

    public static readonly DiagnosticDescriptor ReplaceSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule = new DiagnosticDescriptor(
        id: "SHARPEN030",
        title: "Replace switch statement with switch expression",
        messageFormat: "Replace switch statement with switch expression",
        category: "Sharpen.CSharp8",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "This switch statement can be safely replaced with a switch expression."
    );

    public static readonly DiagnosticDescriptor AwaitEquivalentAsynchronousMethodRule = new DiagnosticDescriptor(
        id: "SHARPEN003",
        title: "Use async equivalent",
        messageFormat: "Use '{0}Async' instead of '{0}' and await it.",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using async equivalents improves responsiveness and scalability."
    );

    public static readonly DiagnosticDescriptor ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule = new DiagnosticDescriptor(
        id: "SHARPEN009",
        title: "Consider awaiting equivalent asynchronous method and making the caller asynchronous",
        messageFormat: "Consider using '{0}Async' instead of '{0}' and await it.",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using async equivalents improves responsiveness and scalability."
    );

    public static readonly DiagnosticDescriptor AwaitTaskDelayInsteadOfCallingThreadSleepRule = new DiagnosticDescriptor(
        id: "SHARPEN004",
        title: "Await Task.Delay instead of calling Thread.Sleep",
        messageFormat: "Replace 'Thread.Sleep' with 'await Task.Delay'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.Delay avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskResultRule = new DiagnosticDescriptor(
        id: "SHARPEN005",
        title: "Await Task instead of calling Task.Result",
        messageFormat: "Replace '.Result' with 'await'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using await avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskWaitRule = new DiagnosticDescriptor(
        id: "SHARPEN006",
        title: "Await Task instead of calling Task.Wait",
        messageFormat: "Replace '.Wait()' with 'await'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using await avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule = new DiagnosticDescriptor(
        id: "SHARPEN007",
        title: "Await Task.WhenAny instead of calling Task.WaitAny",
        messageFormat: "Replace 'Task.WaitAny' with 'await Task.WhenAny'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.WhenAny avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule = new DiagnosticDescriptor(
        id: "SHARPEN008",
        title: "Await Task.WhenAll instead of calling Task.WaitAll",
        messageFormat: "Replace 'Task.WaitAll' with 'await Task.WhenAll'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.WhenAll avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetOnlyPropertiesRule = new DiagnosticDescriptor(
        id: "SHARPEN010",
        title: "Use expression-bodied member for get-only property",
        messageFormat: "Use expression-bodied member syntax for this get-only property",
        category: "Sharpen.CSharp6",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple get-only properties."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetOnlyIndexersRule = new DiagnosticDescriptor(
        id: "SHARPEN011",
        title: "Use expression-bodied member for get-only indexer",
        messageFormat: "Use expression-bodied member syntax for this get-only indexer",
        category: "Sharpen.CSharp6",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple get-only indexers."
    );

    public static readonly DiagnosticDescriptor UseNameofExpressionForThrowingArgumentExceptionsRule = new DiagnosticDescriptor(
        id: "SHARPEN012",
        title: "Use nameof expression for parameter name",
        messageFormat: "Use nameof({0}) instead of string literal",
        category: "Sharpen.CSharp6",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using nameof avoids magic strings and keeps parameter names refactoring-safe."
    );

    public static readonly DiagnosticDescriptor UseNameofExpressionInDependencyPropertyDeclarationsRule = new DiagnosticDescriptor(
        id: "SHARPEN013",
        title: "Use nameof expression for dependency property name",
        messageFormat: "Use nameof({0}) instead of string literal",
        category: "Sharpen.CSharp6",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using nameof avoids magic strings and keeps dependency property names refactoring-safe."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetAccessorsInPropertiesRule = new DiagnosticDescriptor(
        id: "SHARPEN014",
        title: "Use expression-bodied member for get accessor in property",
        messageFormat: "Use expression-bodied member syntax for this get accessor",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple property accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetAccessorsInIndexersRule = new DiagnosticDescriptor(
        id: "SHARPEN042",
        title: "Use expression-bodied member for get accessor in indexer",
        messageFormat: "Use expression-bodied member syntax for this get accessor",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple indexer accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForSetAccessorsInIndexersRule = new DiagnosticDescriptor(
        id: "SHARPEN055",
        title: "Use expression-bodied member for set accessor in indexer",
        messageFormat: "Use expression-bodied member syntax for this set accessor",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple indexer accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForConstructorsRule = new DiagnosticDescriptor(
        id: "SHARPEN015",
        title: "Use expression-bodied member for constructor",
        messageFormat: "Use expression-bodied member syntax for this constructor",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple constructors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForDestructorsRule = new DiagnosticDescriptor(
        id: "SHARPEN016",
        title: "Use expression-bodied member for destructor",
        messageFormat: "Use expression-bodied member syntax for this destructor",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple destructors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForLocalFunctionsRule = new DiagnosticDescriptor(
        id: "SHARPEN017",
        title: "Use expression-bodied member for local function",
        messageFormat: "Use expression-bodied member syntax for this local function",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Expression-bodied members can improve readability for simple local functions."
    );

    public static readonly DiagnosticDescriptor UseOutVariablesInMethodInvocationsRule = new DiagnosticDescriptor(
        id: "SHARPEN018",
        title: "Use out variables in method invocations",
        messageFormat: "Use out variables in this method invocation",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Out variables reduce the need for separate variable declarations when calling methods with out parameters."
    );

    public static readonly DiagnosticDescriptor UseOutVariablesInObjectCreationsRule = new DiagnosticDescriptor(
        id: "SHARPEN019",
        title: "Use out variables in object creations",
        messageFormat: "Use out variables in this object creation",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Out variables reduce the need for separate variable declarations when calling constructors with out parameters."
    );

    public static readonly DiagnosticDescriptor DiscardOutVariablesInMethodInvocationsRule = new DiagnosticDescriptor(
        id: "SHARPEN020",
        title: "Discard out variables in method invocations",
        messageFormat: "Discard unused out variables in this method invocation",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Discarding unused out variables improves readability by avoiding unused local variables."
    );

    public static readonly DiagnosticDescriptor DiscardOutVariablesInObjectCreationsRule = new DiagnosticDescriptor(
        id: "SHARPEN021",
        title: "Discard out variables in object creations",
        messageFormat: "Discard unused out variables in this object creation",
        category: "Sharpen.CSharp7",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Discarding unused out variables improves readability by avoiding unused local variables."
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInReturnStatementsRule = new DiagnosticDescriptor(
        id: "SHARPEN022",
        title: "Use default expression in return statements",
        messageFormat: "Use 'default' instead of 'default({0})'",
        category: "Sharpen.CSharp71",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 7.1 allows using the default literal instead of default(T) when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInOptionalMethodParametersRule = new DiagnosticDescriptor(
        id: "SHARPEN023",
        title: "Use default expression in optional method parameters",
        messageFormat: "Use 'default' instead of 'default({0})'",
        category: "Sharpen.CSharp71",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 7.1 allows using the default literal instead of default(T) in optional parameters when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInOptionalConstructorParametersRule = new DiagnosticDescriptor(
        id: "SHARPEN024",
        title: "Use default expression in optional constructor parameters",
        messageFormat: "Use 'default' instead of 'default({0})'",
        category: "Sharpen.CSharp71",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "C# 7.1 allows using the default literal instead of default(T) in optional parameters when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );
}
