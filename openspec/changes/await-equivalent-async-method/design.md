## Context

Sharpen currently has an analyzer/code fix pair for suggesting the use of an asynchronous equivalent method when the caller is already `async`.

- The analyzer uses [`HardcodedLookupBasedEquivalentAsynchronousMethodFinder`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/HardcodedLookupBasedEquivalentAsynchronousMethodFinder.cs:15) and the shared logic in [`EquivalentAsynchronousMethodFinder.EquivalentAsynchronousCandidateExistsFor()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs:141) to decide whether a synchronous invocation has a valid async equivalent in the current context.
- The current code fix provider attempts to find an async method by name and then applies a simplified compatibility check. This can diverge from the analyzer’s resolution rules (extension methods, partial types, optional `CancellationToken`, and strict parameter name matching).

The goal of this change is to make the code fix resolve and apply the same async equivalent that the analyzer logic is based on, so that diagnostics are consistently actionable.

Constraints and considerations:
- The analyzer’s finder logic is already present in this repo under `Sharpen.Engine.SharpenSuggestions.Common.AsyncAwaitAndAsyncStreams`.
- The code fix must avoid producing invalid syntax (e.g., adding `await` where it is not allowed, or double-awaiting).
- The rule is scoped to callers that are already `async` (matching the old engine suggestion configuration).

## Goals / Non-Goals

**Goals:**
- Reuse the existing equivalent-async resolution rules (symbol-based) for the code fix, not a separate heuristic.
- Correctly resolve async equivalents in these common cases:
  - instance methods
  - static methods
  - extension methods (including reduced extension methods)
  - partial types where `LookupSymbols` position matters
- Apply a safe rewrite strategy:
  - replace the invoked method name with the resolved async equivalent name
  - add `await` only when needed and only when syntactically valid
  - avoid double-await
- Expand tests to cover representative invocation shapes and edge cases.

**Non-Goals:**
- Making non-async callers async (that is a different suggestion family).
- Supporting lambdas/anonymous methods for this suggestion (the shared finder currently excludes them).
- Introducing new user-configurable mappings for async equivalents (continue using the hardcoded heuristic + symbol checks).

## Decisions

1) **Single source of truth for async-equivalent resolution**

- Decision: Implement a helper used by the code fix that mirrors the analyzer’s resolution path in [`EquivalentAsynchronousMethodFinder`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs:141), but returns the chosen `IMethodSymbol` (or null) rather than only `bool`.
- Rationale: The analyzer already encodes the project’s intended definition of “equivalent async method” (return type rules, parameter rules, optional `CancellationToken`, extension method lookup). The code fix must not drift.
- Alternatives considered:
  - Keep the current `Name + Async` + local compatibility checks: rejected due to drift and missed extension/partial cases.
  - Hardcode a mapping table: rejected because the engine already provides a robust symbol-based check.

2) **Candidate search strategy**

- Decision: Search for candidates using the same two-type approach as the analyzer:
  - invoked method’s containing type
  - receiver type (if any) when different
  and use `semanticModel.LookupSymbols(... includeReducedExtensionMethods: true)` at the invocation position.
- Rationale: This matches the analyzer’s behavior and supports extension methods and partial types.

3) **Rewrite strategy for adding `await`**

- Decision: Add `await` only when:
  - the invocation is not already within an `await` expression
  - the enclosing callable is `async` (this rule’s precondition)
  - the invocation is in an expression position where `await` is legal (e.g., expression statement, assignment RHS, return expression)
- Rationale: Prevent invalid code and double-await.
- Alternatives considered:
  - Always wrap with `await` when enclosing method is async: rejected because it can create invalid syntax in some contexts.

4) **Return statement handling**

- Decision: Prefer `return await XAsync(...);` when the original code was `return X(...);` inside an async method returning `Task<T>`.
- Rationale: This preserves the original return shape and avoids changing the method’s return type usage patterns.
- Alternative: `return XAsync(...);` can be valid and sometimes preferred, but changes semantics around exception timing and stack traces; we keep the conservative approach.

## Risks / Trade-offs

- **Risk:** Multiple async overloads match by name but differ subtly.
  - Mitigation: Use the same strict equivalence predicate as the analyzer (return type + parameter type/name + optional `CancellationToken`).

- **Risk:** Code fix introduces invalid syntax by inserting `await` in unsupported contexts.
  - Mitigation: Restrict rewrite to known-safe contexts and add tests for each supported context.

- **Risk:** Extension method resolution differs depending on using directives and scope.
  - Mitigation: Use `LookupSymbols` at the invocation position with `includeReducedExtensionMethods: true`, matching analyzer behavior.

- **Trade-off:** Conservative rewrite may skip some fixable cases (e.g., complex expressions) initially.
  - Mitigation: Start with safe contexts; expand later with additional syntax handling and tests.
