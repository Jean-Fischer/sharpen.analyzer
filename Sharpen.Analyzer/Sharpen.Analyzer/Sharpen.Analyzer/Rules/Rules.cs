using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class Rules
{
    public static readonly DiagnosticDescriptor UseVarKeywordRule = new DiagnosticDescriptor(
        id: "SHARPEN002",
        title: "Use var keyword in variable declaration with object creation",
        messageFormat: "Use 'var' instead of explicit type '{0}' for this declaration",
        category: "Sharpen.CSharp3",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Using 'var' improves readability and reduces redundancy when the type is obvious from the right-hand side."
    );

    public static readonly DiagnosticDescriptor AwaitEquivalentAsynchronousMethodRule = new DiagnosticDescriptor(
        id: "SHARPEN003",
        title: "Use async equivalent",
        messageFormat: "Use '{0}Async' instead of '{0}' and await it.",
        category: "Sharpen.AsyncAwait",
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
