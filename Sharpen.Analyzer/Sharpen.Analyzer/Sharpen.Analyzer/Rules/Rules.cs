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

    public static DiagnosticDescriptor ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule = new DiagnosticDescriptor(
        id: "SHARPEN009",
        title: "Consider awaiting equivalent asynchronous method and making the caller asynchronous",
        messageFormat: "Consider using '{0}Async' instead of '{0}' and await it.",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using async equivalents improves responsiveness and scalability."
    );

    public static DiagnosticDescriptor AwaitTaskDelayInsteadOfCallingThreadSleepRule = new DiagnosticDescriptor(
        id: "SHARPEN004",
        title: "Await Task.Delay instead of calling Thread.Sleep",
        messageFormat: "Replace 'Thread.Sleep' with 'await Task.Delay'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.Delay avoids blocking threads and enables async execution."
    );

    public static DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskResultRule = new DiagnosticDescriptor(
        id: "SHARPEN005",
        title: "Await Task instead of calling Task.Result",
        messageFormat: "Replace '.Result' with 'await'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using await avoids blocking threads and enables async execution."
    );

    public static DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskWaitRule = new DiagnosticDescriptor(
        id: "SHARPEN006",
        title: "Await Task instead of calling Task.Wait",
        messageFormat: "Replace '.Wait()' with 'await'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using await avoids blocking threads and enables async execution."
    );

    public static DiagnosticDescriptor AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule = new DiagnosticDescriptor(
        id: "SHARPEN007",
        title: "Await Task.WhenAny instead of calling Task.WaitAny",
        messageFormat: "Replace 'Task.WaitAny' with 'await Task.WhenAny'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.WhenAny avoids blocking threads and enables async execution."
    );

    public static DiagnosticDescriptor AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule = new DiagnosticDescriptor(
        id: "SHARPEN008",
        title: "Await Task.WhenAll instead of calling Task.WaitAll",
        messageFormat: "Replace 'Task.WaitAll' with 'await Task.WhenAll'",
        category: "Sharpen.CSharp5",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Using Task.WhenAll avoids blocking threads and enables async execution."
    );
}
