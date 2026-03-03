## Why

The new `Sharpen.Analyzer` solution currently contains only a subset of the analyzers and code fixes that exist in the original Sharpen repository.

The original C# 5.0 smoke suite (`original-sharpen/tests/smoke/CSharp50`) encodes a set of async/await modernization recommendations (and their edge cases) that we want to preserve in the new analyzer project.

Migrating these analyzers and fix providers (and recreating their tests) ensures:

- Feature parity with the original Sharpen analyzers for the C# 5.0 async/await rules.
- Regression protection via unit tests that reflect the original smoke scenarios.
- A consistent developer experience in the new solution (`Sharpen.Analyzer/Sharpen.Analyzer.sln`).

## What Changes

- Add the remaining C# 5.0 async/await analyzers represented by the original smoke suite under [`original-sharpen/tests/smoke/CSharp50`](original-sharpen/tests/smoke/CSharp50:1).
- Add corresponding code fix providers where a safe and deterministic fix can be offered.
- Add unit tests in `Sharpen.Analyzer.Tests` that cover:
  - Positive cases (diagnostics are reported) using the original "CanBe..." smoke files.
  - Negative cases (no diagnostics) using the original "CannotBe..." smoke files.
  - Code fix behavior for supported fixes.

Scope (based on [`original-sharpen/tests/smoke/CSharp50/CSharp50.csproj`](original-sharpen/tests/smoke/CSharp50/CSharp50.csproj:1)):

- AwaitTaskDelayInsteadOfCallingThreadSleep
- AwaitTaskInsteadOfCallingTaskResult
- AwaitTaskInsteadOfCallingTaskWait
- AwaitTaskWhenAnyInsteadOfCallingTaskWaitAny
- AwaitTaskWhenAllInsteadOfCallingTaskWaitAll
- ConsiderAwaitingEquivalentAsynchronousMethodAndMakingTheCallerAsynchronous

Note: `AwaitEquivalentAsynchronousMethodAnalyzer` is already migrated and is not part of this change.

## Capabilities

### New Capabilities

- `await-task-delay-instead-of-thread-sleep`: Report diagnostics (and provide a code fix where applicable) when `Thread.Sleep(...)` is used in an async-capable context and can be replaced by `await Task.Delay(...)`.
- `await-task-instead-of-task-result`: Report diagnostics (and provide a code fix where applicable) when `Task<T>.Result` is used in an async-capable context and can be replaced by `await`.
- `await-task-instead-of-task-wait`: Report diagnostics (and provide a code fix where applicable) when `Task.Wait(...)` is used in an async-capable context and can be replaced by `await`.
- `await-task-when-any-instead-of-task-wait-any`: Report diagnostics (and provide a code fix where applicable) when `Task.WaitAny(...)` is used in an async-capable context and can be replaced by `await Task.WhenAny(...)`.
- `await-task-when-all-instead-of-task-wait-all`: Report diagnostics (and provide a code fix where applicable) when `Task.WaitAll(...)` is used in an async-capable context and can be replaced by `await Task.WhenAll(...)`.
- `consider-awaiting-equivalent-async-method-and-making-caller-async`: Report diagnostics (and provide a code fix where applicable) when a synchronous method invocation has an equivalent asynchronous method and the caller can be made async to await it.

### Modified Capabilities

- `await-equivalent-async-method`: Extend/align existing behavior and tests if needed to share infrastructure with the new C# 5.0 async/await rules (no spec-level behavior change intended; this is primarily implementation reuse).

## Impact

- New analyzer and code fix provider classes will be added under the `Sharpen.Analyzer` project (likely alongside existing analyzers such as [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/AwaitEquivalentAsynchronousMethodAnalyzer.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/AwaitEquivalentAsynchronousMethodAnalyzer.cs:1)).
- New diagnostic descriptors will be added to the rules catalog (currently in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs:1)).
- New unit tests will be added to `Sharpen.Analyzer.Tests` following the existing test patterns (e.g. [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/AwaitEquivalentAsynchronousMethodCodeFixTests.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/AwaitEquivalentAsynchronousMethodCodeFixTests.cs:1)).
- Some rules may require explicit decisions about code-fix scope (for example, `Task.WaitAny` return value semantics and `Task.Wait(timeout)` overloads). Where a safe fix is not possible, the analyzer may remain diagnostic-only.
