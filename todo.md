TODO: Review whether the C# 13 test verifier should use real reference assemblies (e.g., a future `ReferenceAssemblies.Net.NetX_Y` that includes `System.Threading.Lock`) instead of the current minimal stub in [`CSharp13CodeFixVerifier<TAnalyzer, TCodeFix>`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure/CSharp13CodeFixVerifier.cs:1).

## C# 13 implementation follow-ups (shortcuts / known limitations)

- **Partial properties/indexers refactoring: indexers excluded**
  - Limitation: [`PartialPropertiesIndexersRefactoringAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp13/PartialPropertiesIndexersRefactoringAnalyzer.cs:11) currently returns `false` for indexers and only suggests auto-properties.
  - Why: Roslyn preview support in this repo makes it hard to safely test “auto-indexers without bodies”, so indexers were intentionally excluded (see analyzer comment).
  - Long-term fix: Add indexer support end-to-end (analyzer + safety checker + code fix + tests) once the test infrastructure can compile/verify partial indexers reliably.
  - Pointers: [`PartialPropertiesIndexersRefactoringAnalyzer.IsCandidate()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp13/PartialPropertiesIndexersRefactoringAnalyzer.cs:53), [`PartialPropertiesIndexersRefactoringCodeFixProvider.CreateImplementingDeclaration()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/FixProvider/CSharp13/PartialPropertiesIndexersRefactoringCodeFixProvider.cs:104), [`PartialPropertiesIndexersRefactoringCodeFixTests`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/PartialPropertiesIndexersRefactoringCodeFixTests.cs:11).

- **C# 13 test infrastructure: `CSharp13CodeFixVerifier` pins `ReferenceAssemblies.Net.Net90`**
  - Limitation: Tests using [`CSharp13CodeFixVerifier<TAnalyzer, TCodeFix>`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure/CSharp13CodeFixVerifier.cs:20) compile against .NET 9 reference assemblies regardless of the repo’s baseline.
  - Why: Needed a “modern target framework” so preview C# 13 features that depend on runtime support can compile in fixed-state (see comment in verifier).
  - Long-term fix: Revisit whether .NET 9 is the right baseline for tests (or whether it should be conditional / centralized), and switch to the smallest reference set that still supports the required C# 13 features.
  - Pointers: [`CSharp13CodeFixVerifier.CreateTest()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure/CSharp13CodeFixVerifier.cs:70).

- **`System.Threading.Lock` is injected as a test stub**
  - Limitation: The verifier injects a minimal `System.Threading.Lock` implementation into both test and fixed states, which can mask real API shape differences.
  - Why: Current reference assemblies used by tests don’t provide `System.Threading.Lock`, but safety checkers/code fixes need the symbol to resolve.
  - Long-term fix: Remove the stub and rely on real reference assemblies once they include `System.Threading.Lock` (or use a dedicated reference assembly package that contains it).
  - Pointers: [`CSharp13CodeFixVerifier.LockStubSource`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure/CSharp13CodeFixVerifier.cs:93), [`UseSystemThreadingLockSafetyChecker`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/UseSystemThreadingLockSafetyChecker.cs:9).

# Inventory: original-sharpen vs sharpen.analyzer

## Scope
You asked for an inventory of **all analyzers** and **fix providers** in the old project [`./original-sharpen`](original-sharpen:1) and a comparison with what is implemented in the new project [`./sharpen.analyzer`](Sharpen.Analyzer:1).

Important terminology mismatch:
- In [`original-sharpen`](original-sharpen:1), “analyzers” are *engine analyzers* implementing [`ISingleSyntaxTreeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/ISingleSyntaxTreeAnalyzer.cs:6) and returning `AnalysisResult` objects. “Fixes” are *suggestions* (`ISharpenSuggestion`) surfaced by the VS extension UI.
- In [`sharpen.analyzer`](Sharpen.Analyzer:1), analyzers are Roslyn [`DiagnosticAnalyzer`](Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer:1) implementations and fixes are Roslyn [`CodeFixProvider`](Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider:1) implementations.

This report compares **capabilities** by:
1) enumerating the old engine analyzers from [`SharpenAnalyzersHolder.Analyzers`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21)
2) verifying the corresponding new analyzer/fix exists, and
3) spot-checking **implementation content** (not only file names) for a few representative rules.

## Findings

### 1) Old project (`original-sharpen`): where analysis/fixes live
The old implementation is split into:

- **VS integration layer**: [`original-sharpen/src/Sharpen.VisualStudioExtension`](original-sharpen/src/Sharpen.VisualStudioExtension:1)
  - Collects scope (solution/projects/documents) and runs analysis via scope analyzers like [`SolutionScopeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/SolutionScopeAnalyzer.cs:1) and [`MultipleDocumentsScopeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/MultipleDocumentsScopeAnalyzer.cs:1).

- **Analysis + suggestion engine**: [`original-sharpen/src/Sharpen.Engine`](original-sharpen/src/Sharpen.Engine:1)
  - Central registry of analyzers: [`SharpenAnalyzersHolder.Analyzers`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21)
  - Execution model: [`BaseScopeAnalyzer`](original-sharpen/src/Sharpen.Engine/Analysis/BaseScopeAnalyzer.cs:12) iterates documents, builds a [`SingleSyntaxTreeAnalysisContext`](original-sharpen/src/Sharpen.Engine/Analysis/BaseScopeAnalyzer.cs:77), and runs all analyzers in parallel (`Parallel.Invoke`) against the same `SyntaxTree` + `SemanticModel`.

### 2) Old project inventory: Engine analyzers (and their suggestions)
From [`SharpenAnalyzersHolder.Analyzers`](original-sharpen/src/Sharpen.Engine/Analysis/SharpenAnalyzersHolder.cs:21), the old project contains the following capabilities:

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
  - Code fix provider exists: [`EnableNullableContextAndDeclareIdentifierAsNullableCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp8/EnableNullableContextAndDeclareIdentifierAsNullableCodeFixProvider.cs:1)
  - Code fix test: not implemented (Roslyn test harness rejects it as "non-local analyzer diagnostic" because the analyzer reports the diagnostic on the declaration location while being triggered by a different usage site)
- `ConsiderAwaitingEquivalentAsynchronousMethodAndYieldingIAsyncEnumerableAnalyzer`
- `ReplaceUsingStatementWithUsingDeclarationAnalyzer`
- `ReplaceSwitchStatementWithSwitchExpressionAnalyzer`
- `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer`

### 3) New project (`sharpen.analyzer`): analyzers / code fix providers
The new project contains Roslyn analyzers and code fix providers under:
- Analyzers: [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/:1)
- Fix providers: [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/:1)

Additionally, the new project includes analyzers beyond the old project (C# 9-12), e.g. [`UseTopLevelStatementsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp9/UseTopLevelStatementsAnalyzer.cs:11), [`UsePrimaryConstructorAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp12/UsePrimaryConstructorAnalyzer.cs:11).

### 4) Content-based comparison (spot checks)

#### 4.1 Out variables: object creations
Old engine:
- [`UseOutVariablesInObjectCreations`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/OutVariables/UseOutVariablesInObjectCreations.cs:5) is a thin wrapper over [`BaseUseOutVariables<T>`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/OutVariables/BaseUseOutVariables.cs:21) with `base(false)` meaning “use out var” (not discard).

New Roslyn analyzer:
- [`UseOutVariablesInObjectCreationsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseOutVariablesInObjectCreationsAnalyzer.cs:12) registers on `SyntaxKind.ObjectCreationExpression` and:
  - iterates arguments
  - filters `out` keyword + identifier expression
  - calls [`OutVariableCandidateHelper.IsCandidate(...)`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseOutVariablesInObjectCreationsAnalyzer.cs:47) with `outArgumentCanBeDiscarded: false`
  - reports diagnostic on the `out` keyword location

Conclusion: capability exists in new project and the core detection logic is present.

#### 4.2 Out variables: discard in method invocations
Old engine:
- [`DiscardOutVariablesInMethodInvocations`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/OutVariables/DiscardOutVariablesInMethodInvocations.cs:5) is also a thin wrapper over `BaseUseOutVariables<T>` but with `base(true)` meaning “discard allowed/desired”.

New Roslyn analyzer:
- [`DiscardOutVariablesInMethodInvocationsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/DiscardOutVariablesInMethodInvocationsAnalyzer.cs:12) registers on `SyntaxKind.InvocationExpression` and:
  - iterates arguments
  - filters `out` keyword + identifier expression
  - calls `OutVariableCandidateHelper.IsCandidate(..., outArgumentCanBeDiscarded: true)`
  - reports diagnostic on the `out` keyword location

Conclusion: capability exists in new project and the “discard vs out var” distinction is preserved.

#### 4.3 Expression-bodied set accessor in indexers
New Roslyn code fix:
- [`UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp7/UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider.cs:16) performs a concrete syntax transformation:
  - finds the `AccessorDeclarationSyntax` at diagnostic span
  - validates it is a `set` accessor inside an `IndexerDeclarationSyntax`
  - requires a block body with exactly one `ExpressionStatementSyntax`
  - replaces `{ expr; }` with `=> expr;` while attempting to preserve trivia

Conclusion: new project not only detects but also provides an automated fix for this rule.

#### 4.4 Expression-bodied set accessor in properties (deeper check)
Old engine:
- The rule is implemented by [`UseExpressionBodyForSetAccessorsInProperties`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/ExpressionBodiedMembers/UseExpressionBodyForSetAccessorsInProperties.cs:5), which inherits [`BaseUseExpressionBodyForSetAccessors<TBasePropertyDeclarationSyntax>`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/ExpressionBodiedMembers/BaseUseExpressionBodyForSetAccessors.cs:10).
- The detection logic in the base class is:
  - scan all [`AccessorDeclarationSyntax`](Microsoft.CodeAnalysis.CSharp.Syntax.AccessorDeclarationSyntax:1)
  - keep only `set` accessors (`accessor.Keyword.IsKind(SyntaxKind.SetKeyword)`)
  - require a block body with exactly one statement
  - require that statement is an expression statement
  - require the accessor is inside a `PropertyDeclarationSyntax` (for this specialization)

New project:
- There is a matching analyzer+rule+codefix for **indexers** only:
  - analyzer: [`UseExpressionBodyForSetAccessorsInIndexersAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseExpressionBodyForSetAccessorsInIndexersAnalyzer.cs:11)
  - rule descriptor: [`UseExpressionBodyForSetAccessorsInIndexersRule`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs:298)
  - code fix: [`UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp7/UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider.cs:16)
- A repository-wide search for `UseExpressionBodyForSetAccessorsInProperties` returns no results, and there is no `DiagnosticDescriptor` for “set accessor in property” in [`Rules.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs:250).
- A search for analyzers registering on `SyntaxKind.SetAccessorDeclaration` shows only the indexer rule (plus unrelated analyzers like init-only setter) (see [`UseExpressionBodyForSetAccessorsInIndexersAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseExpressionBodyForSetAccessorsInIndexersAnalyzer.cs:21)).

Conclusion: `UseExpressionBodyForSetAccessorsInProperties` appears to be a **real missing capability** in the new project (not just a naming mismatch).

### 5) Delta: “in original but not in new”
After re-checking the new project’s analyzer/fix inventory (and not relying only on file names), the earlier “missing” list was incorrect.

The following capabilities from the old engine are present in the new project (examples):
- `UseExpressionBodyForGetAccessorsInIndexers` exists as [`UseExpressionBodyForGetAccessorsInIndexersAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp6/UseExpressionBodyForGetAccessorsInIndexersAnalyzer.cs:12) + [`UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp6/UseExpressionBodyForGetAccessorsInIndexersCodeFixProvider.cs:17)
- `UseExpressionBodyForSetAccessorsInIndexers` exists as [`UseExpressionBodyForSetAccessorsInIndexersAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseExpressionBodyForSetAccessorsInIndexersAnalyzer.cs:11) + [`UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp7/UseExpressionBodyForSetAccessorsInIndexersCodeFixProvider.cs:16)
- `DiscardOutVariablesInMethodInvocations` exists as [`DiscardOutVariablesInMethodInvocationsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/DiscardOutVariablesInMethodInvocationsAnalyzer.cs:12)
- `DiscardOutVariablesInObjectCreations` exists as [`DiscardOutVariablesInObjectCreationsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/DiscardOutVariablesInObjectCreationsAnalyzer.cs:12)
- `UseOutVariablesInObjectCreations` exists as [`UseOutVariablesInObjectCreationsAnalyzer`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp7/UseOutVariablesInObjectCreationsAnalyzer.cs:12)

Confirmed missing capability:
- `UseExpressionBodyForSetAccessorsInProperties` exists in the old engine list (see [`UseExpressionBodyForSetAccessorsInProperties`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp70/ExpressionBodiedMembers/UseExpressionBodyForSetAccessorsInProperties.cs:5)) but does not appear to exist in the new project (no analyzer, no rule descriptor, no code fix provider).

## Notes / limitations
- The old project’s “fixes” are not Roslyn `CodeFixProvider`s; they are suggestions surfaced by the VS extension UI. So this report compares *capabilities*, not 1:1 type equivalence.
- I only did **content spot-checks** for a subset of rules (out vars + expression-bodied rule(s)). A full content-level diff for every rule would require reading each old suggestion implementation and its new analyzer + code fix pair.
