## Why

The analyzer already detects when a synchronous method call has a valid asynchronous equivalent, but the current code fix does not reliably resolve and apply the same equivalent method that the analyzer logic is based on. This change aligns the code fix behavior with the engine’s equivalent-async resolution rules so diagnostics are actionable and safe.

## What Changes

- Update the `AwaitEquivalentAsynchronousMethod` analyzer/code fix pair so the code fix resolves the async equivalent using the same symbol-based rules as the analyzer (including extension methods, partial types, return-type compatibility, and optional `CancellationToken`).
- Improve code fix rewriting so it adds `await` only when appropriate (avoid double-await; handle common contexts like expression statements, assignments, and returns).
- Expand unit tests to cover representative invocation shapes and edge cases (already-awaited, assignment, return, extension method, ignored methods).

## Capabilities

### New Capabilities
- `await-equivalent-async-method`: Provide a diagnostic and code fix that replaces a synchronous invocation with its equivalent asynchronous invocation and awaits it when required by context.

### Modified Capabilities

- (none)

## Impact

- Affected code:
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/AwaitEquivalentAsynchronousMethodAnalyzer.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/AwaitEquivalentAsynchronousMethodAnalyzer.cs:1)
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/AwaitEquivalentAsynchronousMethodCodeFixProvider.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/AwaitEquivalentAsynchronousMethodCodeFixProvider.cs:1)
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs:1) (reuse/extend for code fix resolution)
  - Tests under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests:1)
- No breaking API changes expected; this is analyzer/code-fix behavior alignment.
- Risk: code fix must avoid producing invalid syntax in certain contexts; mitigated by targeted rewriting rules and tests.
