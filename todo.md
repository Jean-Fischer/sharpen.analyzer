# Inventory: original-sharpen vs sharpen.analyzer

## Scope
You asked for an inventory of **all analyzers** and **fix providers** in the old project [`./original-sharpen`](original-sharpen:1) and a comparison with what is implemented in the new project [`./sharpen.analyzer`](Sharpen.Analyzer:1), to identify what existed in the original but is missing in the new.

Because the old solution is a Visual Studio extension + an internal “engine” (not Roslyn `DiagnosticAnalyzer`/`CodeFixProvider` types), this report treats the old “analyzers” as the Sharpen Engine’s `ISingleSyntaxTreeAnalyzer` implementations and the old “fix providers” as the Sharpen Engine’s suggestion objects (`ISharpenSuggestion`) that describe the recommended change.

## Findings

### 1) Old project (`original-sharpen`): where analysis/fixes live
The old implementation is split into:

- **VS integration layer**: [`original-sharpen/src/Sharpen.VisualStudioExtension`](original-sharpen/src/Sharpen.VisualStudioExtension:1)
  - Uses Roslyn workspace APIs (e.g. [`VisualStudioWorkspace`](original-sharpen/src/Sharpen.VisualStudioExtension/Commands/ICommandServicesContainer.cs:1), [`Document`](original-sharpen/src/Sharpen.VisualStudioExtension/VisualStudioExtensions.cs:1)) to collect documents/projects/solutions.
  - Runs analysis via `Sharpen.Engine.Analysis` scope analyzers (e.g. [`SolutionScopeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/SolutionScopeAnalyzer.cs:1), [`MultipleDocumentsScopeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/MultipleDocumentsScopeAnalyzer.cs:1)).
  - Displays results in a tool window (e.g. [`SharpenResultsToolWindow`](original-sharpen/src/Sharpen.VisualStudioExtension/ToolWindows/SharpenResultsToolWindow.cs:1)).

- **Analysis + “fix suggestion” engine**: [`original-sharpen/src/Sharpen.Engine`](original-sharpen/src/Sharpen.Engine:1)
  - Central registry of analyzers: [`SharpenAnalyzersHolder.Analyzers`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21)

### 2) Old project inventory: Engine analyzers (and their suggestions)
From [`SharpenAnalyzersHolder.Analyzers`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21), the old project contains the following “analyzer/suggestion” capabilities:

#### C# 3.0
- `UseVarKeywordInVariableDeclarationWithObjectCreation`

#### C# 5.0 (Async/Await)
- `ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronous`
- `AwaitEquivalentAsynchronousMethod`
- `AwaitTaskDelayInsteadOfCallingThreadSleep`
- `AwaitTaskInsteadOfCallingTaskWait`
- `AwaitTaskInsteadOfCallingTaskResult`
- `AwaitTaskWhenAllInsteadOfCallingTaskWaitAll`
- `AwaitTaskWhenAnyInsteadOfCallingTaskWaitAny`

#### C# 6.0 (Expression-bodied members, nameof)
- `UseExpressionBodyForGetOnlyProperties`
- `UseExpressionBodyForGetOnlyIndexers`
- `UseNameofExpressionForThrowingArgumentExceptions`
- `UseNameofExpressionInDependencyPropertyDeclarations`

#### C# 7.0 (Expression-bodied members, out vars)
- `UseExpressionBodyForConstructors`
- `UseExpressionBodyForDestructors`
- `UseExpressionBodyForGetAccessorsInProperties`
- `UseExpressionBodyForGetAccessorsInIndexers`
- `UseExpressionBodyForSetAccessorsInProperties`
- `UseExpressionBodyForSetAccessorsInIndexers`
- `UseExpressionBodyForLocalFunctions`
- `UseOutVariablesInMethodInvocations`
- `UseOutVariablesInObjectCreations`
- `DiscardOutVariablesInMethodInvocations`
- `DiscardOutVariablesInObjectCreations`

#### C# 7.1 (default literal)
- `UseDefaultExpressionInReturnStatements`
- `UseDefaultExpressionInOptionalMethodParameters`
- `UseDefaultExpressionInOptionalConstructorParameters`

#### C# 8.0
- `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer`
- `ConsiderAwaitingEquivalentAsynchronousMethodAndYieldingIAsyncEnumerableAnalyzer`
- `ReplaceUsingStatementWithUsingDeclarationAnalyzer`
- `ReplaceSwitchStatementWithSwitchExpressionAnalyzer`
- `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer`

### 3) New project (`sharpen.analyzer`): analyzers / code fix providers
The new project contains Roslyn analyzers and code fix providers under:
- Analyzers: [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/:1)
- Fix providers: [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/:1)

### 4) Delta: “in original but not in new” (based on name matching)
The following old-engine capabilities appear to be **missing** in the new Roslyn-based implementation (no matching analyzer/fix-provider class names found in the new project):

- `UseExpressionBodyForGetAccessorsInIndexers`
- `UseExpressionBodyForSetAccessorsInProperties`
- `UseExpressionBodyForSetAccessorsInIndexers`
- `UseOutVariablesInObjectCreations`
- `DiscardOutVariablesInMethodInvocations`
- `DiscardOutVariablesInObjectCreations`

Everything else listed in [`SharpenAnalyzersHolder`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21) appears to have a corresponding analyzer in the new project (at least by class-name match).

## Notes / limitations
- The delta above is computed by **class-name matching** between old engine “suggestions/analyzers” and new Roslyn analyzers/code-fix providers. Some features may exist under different names in the new project.
- The old project’s “fixes” are not Roslyn `CodeFixProvider`s; they are suggestions surfaced by the VS extension UI. So this report compares *capabilities*, not 1:1 type equivalence.
