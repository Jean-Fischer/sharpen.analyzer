using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.Rules;

public static class Rules
{
    // C# 3
    public static readonly DiagnosticDescriptor UseVarKeywordRule = new(
        "SHARPEN002",
        "Use var keyword in variable declaration with object creation",
        "Use 'var' instead of explicit type '{0}' for this declaration",
        "Sharpen.CSharp3",
        DiagnosticSeverity.Info,
        true,
        "Using 'var' improves readability and reduces redundancy when the type is obvious from the right-hand side."
    );

    public static readonly DiagnosticDescriptor UseInitOnlySetterRule = new(
        "SHARPEN035",
        "Use init-only setter",
        "Use 'init' instead of 'private set' for this auto-property",
        "Sharpen.CSharp9",
        DiagnosticSeverity.Info,
        true,
        "C# 9 introduces init-only setters to express immutability after initialization."
    );

    public static readonly DiagnosticDescriptor UseRecordTypeRule = new(
        "SHARPEN036",
        "Use record type",
        "Convert this sealed data class to a record",
        "Sharpen.CSharp9",
        DiagnosticSeverity.Info,
        true,
        "C# 9 records provide concise syntax and value-based semantics for data-centric types."
    );

    public static readonly DiagnosticDescriptor UseTopLevelStatementsRule = new(
        "SHARPEN037",
        "Use top-level statements",
        "Convert this entry point to top-level statements",
        "Sharpen.CSharp9",
        DiagnosticSeverity.Info,
        true,
        "C# 9 top-level statements can reduce boilerplate for simple programs."
    );

    public static readonly DiagnosticDescriptor UseCSharp9PatternMatchingRule = new(
        "SHARPEN038",
        "Use C# 9 pattern matching",
        "Rewrite this expression using C# 9 pattern matching",
        "Sharpen.CSharp9",
        DiagnosticSeverity.Info,
        true,
        "C# 9 adds relational and logical patterns that can simplify boolean expressions."
    );

    public static readonly DiagnosticDescriptor UseTargetTypedNewRule = new(
        "SHARPEN039",
        "Use target-typed new",
        "Use target-typed 'new'",
        "Sharpen.CSharp9",
        DiagnosticSeverity.Info,
        true,
        "C# 9 target-typed new expressions can reduce redundancy when the type is obvious from context."
    );

    public static readonly DiagnosticDescriptor ReplaceUsingStatementWithUsingDeclarationRule = new(
        "SHARPEN025",
        "Replace using statement with using declaration",
        "Replace using statement with using declaration",
        "Sharpen.CSharp8",
        DiagnosticSeverity.Info,
        true,
        "C# 8 allows using declarations which can reduce nesting and improve readability."
    );

    public static readonly DiagnosticDescriptor ReplaceSwitchStatementWithSwitchExpressionRule = new(
        "SHARPEN026",
        "Replace switch statement with switch expression",
        "Replace switch statement with switch expression",
        "Sharpen.CSharp8",
        DiagnosticSeverity.Info,
        true,
        "C# 8 switch expressions can reduce boilerplate and improve readability."
    );

    public static readonly DiagnosticDescriptor EnableNullableContextAndDeclareIdentifierAsNullableRule = new(
        "SHARPEN033",
        "Enable nullable context and declare identifier as nullable",
        "Enable nullable context and declare identifier as nullable",
        "Sharpen.CSharp8",
        DiagnosticSeverity.Info,
        true,
        "C# 8 introduces nullable reference types. Enabling nullable context and marking identifiers as nullable can improve null-safety."
    );

    public static readonly DiagnosticDescriptor UseIndexFromTheEndRule = new(
        "SHARPEN034",
        "Use index from the end",
        "Use index from the end",
        "Sharpen.CSharp8",
        DiagnosticSeverity.Info,
        true,
        "C# 8 introduces indices and ranges. This rule is currently a placeholder for the migrated legacy analyzer."
    );

    public static readonly DiagnosticDescriptor
        UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule = new(
            "SHARPEN031",
            "Use ??= operator instead of assigning result of the ?? operator",
            "Use ??= operator instead of assigning result of the ?? operator",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "C# 8 introduces the ??= operator which can simplify null-coalescing assignments."
        );

    public static readonly DiagnosticDescriptor
        ConsiderUsingNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorRule = new(
            "SHARPEN032",
            "Consider using ??= operator instead of assigning result of the ?? operator",
            "Consider using ??= operator instead of assigning result of the ?? operator",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "This assignment may be replaceable with ??=, but conversion could change semantics due to side effects."
        );

    public static readonly DiagnosticDescriptor
        ConsiderReplacingSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule = new(
            "SHARPEN027",
            "Consider replacing switch statement with switch expression",
            "Consider replacing switch statement with switch expression",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "This switch statement may be convertible to a switch expression, but conversion could change semantics."
        );

    public static readonly DiagnosticDescriptor
        ReplaceSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule = new(
            "SHARPEN028",
            "Replace switch statement with switch expression",
            "Replace switch statement with switch expression",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "This switch statement can be safely replaced with a switch expression."
        );

    public static readonly DiagnosticDescriptor
        ConsiderReplacingSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule = new(
            "SHARPEN029",
            "Consider replacing switch statement with switch expression",
            "Consider replacing switch statement with switch expression",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "This switch statement may be convertible to a switch expression, but conversion could change semantics."
        );

    public static readonly DiagnosticDescriptor ReplaceSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule =
        new(
            "SHARPEN030",
            "Replace switch statement with switch expression",
            "Replace switch statement with switch expression",
            "Sharpen.CSharp8",
            DiagnosticSeverity.Info,
            true,
            "This switch statement can be safely replaced with a switch expression."
        );

    public static readonly DiagnosticDescriptor AwaitEquivalentAsynchronousMethodRule = new(
        "SHARPEN003",
        "Use async equivalent",
        "Use '{0}Async' instead of '{0}' and await it.",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using async equivalents improves responsiveness and scalability."
    );

    public static readonly DiagnosticDescriptor
        ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronousRule = new(
            "SHARPEN009",
            "Consider awaiting equivalent asynchronous method and making the caller asynchronous",
            "Consider using '{0}Async' instead of '{0}' and await it.",
            "Sharpen.CSharp5",
            DiagnosticSeverity.Warning,
            true,
            "Using async equivalents improves responsiveness and scalability."
        );

    public static readonly DiagnosticDescriptor AwaitTaskDelayInsteadOfCallingThreadSleepRule = new(
        "SHARPEN004",
        "Await Task.Delay instead of calling Thread.Sleep",
        "Replace 'Thread.Sleep' with 'await Task.Delay'",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using Task.Delay avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskResultRule = new(
        "SHARPEN005",
        "Await Task instead of calling Task.Result",
        "Replace '.Result' with 'await'",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using await avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskInsteadOfCallingTaskWaitRule = new(
        "SHARPEN006",
        "Await Task instead of calling Task.Wait",
        "Replace '.Wait()' with 'await'",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using await avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskWhenAnyInsteadOfCallingTaskWaitAnyRule = new(
        "SHARPEN007",
        "Await Task.WhenAny instead of calling Task.WaitAny",
        "Replace 'Task.WaitAny' with 'await Task.WhenAny'",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using Task.WhenAny avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor AwaitTaskWhenAllInsteadOfCallingTaskWaitAllRule = new(
        "SHARPEN008",
        "Await Task.WhenAll instead of calling Task.WaitAll",
        "Replace 'Task.WaitAll' with 'await Task.WhenAll'",
        "Sharpen.CSharp5",
        DiagnosticSeverity.Warning,
        true,
        "Using Task.WhenAll avoids blocking threads and enables async execution."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetOnlyPropertiesRule = new(
        "SHARPEN010",
        "Use expression-bodied member for get-only property",
        "Use expression-bodied member syntax for this get-only property",
        "Sharpen.CSharp6",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple get-only properties."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetOnlyIndexersRule = new(
        "SHARPEN011",
        "Use expression-bodied member for get-only indexer",
        "Use expression-bodied member syntax for this get-only indexer",
        "Sharpen.CSharp6",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple get-only indexers."
    );

    public static readonly DiagnosticDescriptor UseNameofExpressionForThrowingArgumentExceptionsRule = new(
        "SHARPEN012",
        "Use nameof expression for parameter name",
        "Use nameof({0}) instead of string literal",
        "Sharpen.CSharp6",
        DiagnosticSeverity.Info,
        true,
        "Using nameof avoids magic strings and keeps parameter names refactoring-safe."
    );

    public static readonly DiagnosticDescriptor UseNameofExpressionInDependencyPropertyDeclarationsRule = new(
        "SHARPEN013",
        "Use nameof expression for dependency property name",
        "Use nameof({0}) instead of string literal",
        "Sharpen.CSharp6",
        DiagnosticSeverity.Info,
        true,
        "Using nameof avoids magic strings and keeps dependency property names refactoring-safe."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetAccessorsInPropertiesRule = new(
        "SHARPEN014",
        "Use expression-bodied member for get accessor in property",
        "Use expression-bodied member syntax for this get accessor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple property accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForGetAccessorsInIndexersRule = new(
        "SHARPEN057",
        "Use expression-bodied member for get accessor in indexer",
        "Use expression-bodied member syntax for this get accessor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple indexer accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForSetAccessorsInIndexersRule = new(
        "SHARPEN055",
        "Use expression-bodied member for set accessor in indexer",
        "Use expression-bodied member syntax for this set accessor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple indexer accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForSetAccessorsInPropertiesRule = new(
        "SHARPEN056",
        "Use expression-bodied member for set accessor in property",
        "Use expression-bodied member syntax for this set accessor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple property accessors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForConstructorsRule = new(
        "SHARPEN015",
        "Use expression-bodied member for constructor",
        "Use expression-bodied member syntax for this constructor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple constructors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForDestructorsRule = new(
        "SHARPEN016",
        "Use expression-bodied member for destructor",
        "Use expression-bodied member syntax for this destructor",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple destructors."
    );

    public static readonly DiagnosticDescriptor UseExpressionBodyForLocalFunctionsRule = new(
        "SHARPEN017",
        "Use expression-bodied member for local function",
        "Use expression-bodied member syntax for this local function",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Expression-bodied members can improve readability for simple local functions."
    );

    public static readonly DiagnosticDescriptor UseOutVariablesInMethodInvocationsRule = new(
        "SHARPEN018",
        "Use out variables in method invocations",
        "Use out variables in this method invocation",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Out variables reduce the need for separate variable declarations when calling methods with out parameters."
    );

    public static readonly DiagnosticDescriptor UseOutVariablesInObjectCreationsRule = new(
        "SHARPEN019",
        "Use out variables in object creations",
        "Use out variables in this object creation",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Out variables reduce the need for separate variable declarations when calling constructors with out parameters."
    );

    public static readonly DiagnosticDescriptor DiscardOutVariablesInMethodInvocationsRule = new(
        "SHARPEN020",
        "Discard out variables in method invocations",
        "Discard unused out variables in this method invocation",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Discarding unused out variables improves readability by avoiding unused local variables."
    );

    public static readonly DiagnosticDescriptor DiscardOutVariablesInObjectCreationsRule = new(
        "SHARPEN021",
        "Discard out variables in object creations",
        "Discard unused out variables in this object creation",
        "Sharpen.CSharp7",
        DiagnosticSeverity.Info,
        true,
        "Discarding unused out variables improves readability by avoiding unused local variables."
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInReturnStatementsRule = new(
        "SHARPEN022",
        "Use default expression in return statements",
        "Use 'default' instead of 'default({0})'",
        "Sharpen.CSharp71",
        DiagnosticSeverity.Info,
        true,
        "C# 7.1 allows using the default literal instead of default(T) when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInOptionalMethodParametersRule = new(
        "SHARPEN023",
        "Use default expression in optional method parameters",
        "Use 'default' instead of 'default({0})'",
        "Sharpen.CSharp71",
        DiagnosticSeverity.Info,
        true,
        "C# 7.1 allows using the default literal instead of default(T) in optional parameters when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );

    public static readonly DiagnosticDescriptor UseDefaultExpressionInOptionalConstructorParametersRule = new(
        "SHARPEN024",
        "Use default expression in optional constructor parameters",
        "Use 'default' instead of 'default({0})'",
        "Sharpen.CSharp71",
        DiagnosticSeverity.Info,
        true,
        "C# 7.1 allows using the default literal instead of default(T) in optional parameters when the type can be inferred.",
        customTags: new[] { WellKnownDiagnosticTags.Unnecessary }
    );

    // C# 13
    public static readonly DiagnosticDescriptor PreferParamsCollectionsRule = CSharp13Rules.PreferParamsCollectionsRule;

    public static readonly DiagnosticDescriptor UseFromEndIndexInObjectInitializersRule =
        CSharp13Rules.UseFromEndIndexInObjectInitializersRule;

    public static readonly DiagnosticDescriptor UseEscapeSequenceERule = CSharp13Rules.UseEscapeSequenceERule;
    public static readonly DiagnosticDescriptor UseSystemThreadingLockRule = CSharp13Rules.UseSystemThreadingLockRule;

    public static readonly DiagnosticDescriptor PartialPropertiesIndexersRefactoringRule =
        CSharp13Rules.PartialPropertiesIndexersRefactoringRule;

    public static readonly DiagnosticDescriptor SuggestAllowsRefStructConstraintRule =
        CSharp13Rules.SuggestAllowsRefStructConstraintRule;

    public static readonly DiagnosticDescriptor SuggestOverloadResolutionPriorityRule =
        CSharp13Rules.SuggestOverloadResolutionPriorityRule;

    // C# 14
    public static readonly DiagnosticDescriptor UseFieldKeywordInPropertiesRule =
        CSharp14Rules.UseFieldKeywordInPropertiesRule;

    public static readonly DiagnosticDescriptor UseNullConditionalAssignmentRule =
        CSharp14Rules.UseNullConditionalAssignmentRule;

    public static readonly DiagnosticDescriptor UseUnboundGenericTypeInNameofRule =
        CSharp14Rules.UseUnboundGenericTypeInNameofRule;

    public static readonly DiagnosticDescriptor UseLambdaParameterModifiersWithoutTypesRule =
        CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule;

    public static readonly DiagnosticDescriptor UseImplicitSpanConversionsRule =
        CSharp14Rules.UseImplicitSpanConversionsRule;

    public static readonly DiagnosticDescriptor UseExtensionBlocksRule = CSharp14Rules.UseExtensionBlocksRule;
    public static readonly DiagnosticDescriptor UsePartialConstructorsRule = CSharp14Rules.UsePartialConstructorsRule;
    public static readonly DiagnosticDescriptor UsePartialEventsRule = CSharp14Rules.UsePartialEventsRule;

    public static readonly DiagnosticDescriptor SuggestCompoundAssignmentOperatorsRule =
        CSharp14Rules.SuggestCompoundAssignmentOperatorsRule;
}