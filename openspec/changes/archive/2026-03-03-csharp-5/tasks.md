## 1. Inventory & rule mapping

- [x] 1.1 Identify the original Sharpen analyzer/fix implementations for each C# 5.0 rule in [`original-sharpen/tests/smoke/CSharp50`](original-sharpen/tests/smoke/CSharp50:1)
- [x] 1.2 For each rule, list the corresponding smoke files (CanBe*/CannotBe*) and map them to new test cases in `Sharpen.Analyzer.Tests`
- [x] 1.3 Confirm which rules will be diagnostic-only vs offer a code fix (based on safety constraints in [`openspec/changes/csharp-5/design.md`](openspec/changes/csharp-5/design.md:1))

## 2. Shared infrastructure (reuse/extend existing)

- [x] 2.1 Review existing await-equivalent infrastructure (e.g., [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs:1)) and identify reusable helpers for “async-capable context” and “await legality”
- [x] 2.2 Implement/extend shared helpers for:
  - determining whether a containing member can be made `async`
  - determining whether `await` is legal at the diagnostic location
  - applying a consistent “make caller async + add await” transformation
- [x] 2.3 Add unit tests for shared helpers (where feasible) or cover via rule-level tests

## 3. Rule: await-task-delay-instead-of-thread-sleep

- [x] 3.1 Add diagnostic descriptor to rules catalog (e.g., [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs:1))
- [x] 3.2 Implement analyzer to detect `Thread.Sleep(...)` per [`openspec/changes/csharp-5/specs/await-task-delay-instead-of-thread-sleep/spec.md`](openspec/changes/csharp-5/specs/await-task-delay-instead-of-thread-sleep/spec.md:1)
- [x] 3.3 Implement code fix provider to replace with `await Task.Delay(...)` when safe
- [x] 3.4 Add tests for diagnostics (CanBe*/CannotBe* scenarios) and code fix behavior

## 4. Rule: await-task-instead-of-task-result

- [x] 4.1 Add diagnostic descriptor to rules catalog
- [x] 4.2 Implement analyzer to detect `Task<T>.Result` usage in async-capable contexts
- [x] 4.3 Implement code fix provider to replace `.Result` with `await <task>` for safe/simple patterns
- [x] 4.4 Add tests for diagnostics and code fix behavior, including “no fix offered” cases

## 5. Rule: await-task-instead-of-task-wait

- [x] 5.1 Add diagnostic descriptor to rules catalog
- [x] 5.2 Implement analyzer to detect `Task.Wait(...)` usage
- [x] 5.3 Implement code fix provider for parameterless `Wait()` only; ensure overloads with timeout/cancellation are diagnostic-only
- [x] 5.4 Add tests for diagnostics and code fix behavior, including overload negative cases

## 6. Rule: await-task-when-any-instead-of-task-wait-any

- [x] 6.1 Add diagnostic descriptor to rules catalog
- [x] 6.2 Implement analyzer to detect `Task.WaitAny(...)` usage
- [x] 6.3 Implement code fix provider only for statement-only patterns where return value is not used
- [x] 6.4 Add tests for diagnostics and “no fix offered when return value is used”

## 7. Rule: await-task-when-all-instead-of-task-wait-all

- [x] 7.1 Add diagnostic descriptor to rules catalog
- [x] 7.2 Implement analyzer to detect `Task.WaitAll(...)` usage
- [x] 7.3 Implement code fix provider for safe patterns (`await Task.WhenAll(...)`)
- [x] 7.4 Add tests for diagnostics and code fix behavior

## 8. Rule: consider-awaiting-equivalent-async-method-and-making-caller-async

- [x] 8.1 Decide whether to implement as a new analyzer or reuse/alias the existing await-equivalent analyzer (no behavior change intended)
- [x] 8.2 If needed, refactor existing await-equivalent implementation to share infrastructure with the new rules without changing behavior
- [x] 8.3 Add/adjust tests only if required to validate shared infrastructure reuse

## 9. Packaging & verification

- [x] 9.1 Ensure all new analyzers and code fixes are registered/discoverable in the extension packaging
- [x] 9.2 Run full test suite and ensure all new tests pass
- [x] 9.3 Add/update sample code (if applicable) to demonstrate new diagnostics and fixes
