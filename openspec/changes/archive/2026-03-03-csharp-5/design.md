## Context

The new `Sharpen.Analyzer` solution is a partial migration of the original Sharpen repository. For C# 5.0 async/await modernization, the original smoke suite under [`original-sharpen/tests/smoke/CSharp50`](original-sharpen/tests/smoke/CSharp50:1) represents the authoritative set of scenarios and edge cases we want to preserve.

This change migrates the remaining C# 5.0 async/await analyzers and (where safe) code fixes into the new solution, and recreates unit tests in `Sharpen.Analyzer.Tests` based on the original smoke files.

Constraints / considerations:

- The new solution already contains `AwaitEquivalentAsynchronousMethodAnalyzer` and its supporting infrastructure; this change should reuse/align with that implementation where it reduces duplication.
- Some rules have semantics that make a code fix unsafe or ambiguous (e.g., `Task.WaitAny` return value usage, timeout/cancellation overloads). In those cases, the analyzer may be diagnostic-only.
- The migrated analyzers should follow existing patterns in the new codebase (diagnostic descriptors in the rules catalog, analyzers under `Sharpen.Analyzer/Analyzers`, code fixes under `Sharpen.Analyzer/FixProvider`, tests under `Sharpen.Analyzer.Tests`).

## Goals / Non-Goals

**Goals:**

- Provide a consistent architecture for the C# 5.0 async/await rules so they:
  - Share common analysis utilities (e.g., “is this context async-capable?”, “is this invocation replaceable?”).
  - Share common code-fix building blocks (e.g., inserting `await`, adding `async`, adding `using System.Threading.Tasks`, etc.) where applicable.
- Define clear, conservative criteria for when each rule offers a code fix vs diagnostic-only.
- Ensure the migrated behavior is validated by unit tests derived from the original smoke suite.

**Non-Goals:**

- Achieving full feature parity for *all* Sharpen analyzers outside the C# 5.0 async/await scope.
- Implementing risky or behavior-changing code fixes (preference is to be conservative and diagnostic-only when semantics are unclear).
- Refactoring unrelated analyzer infrastructure unless it is required to support the new rules.

## Decisions

### 1) One rule = one analyzer + optional code fix provider

**Decision:** Implement each capability as a dedicated analyzer class and (optionally) a dedicated code fix provider.

**Rationale:**

- Matches existing structure in the new solution and keeps diagnostics and fixes discoverable.
- Allows each rule to have tailored fix-eligibility logic without overcomplicating a shared “mega” code fix.

**Alternatives considered:**

- A single analyzer that emits multiple diagnostics: rejected because it complicates configuration, testing, and code ownership.
- A single code fix provider handling multiple diagnostics: possible, but tends to grow complex; prefer per-rule providers unless strong reuse emerges.

### 2) Shared “async modernization” utilities in `Common/`

**Decision:** Extract shared logic into a small set of internal helpers under `Sharpen.Analyzer/Common` (or reuse existing ones from the already-migrated await-equivalent rule).

Likely shared responsibilities:

- Detecting whether the current method/lambda/local function can be made `async`.
- Determining whether an expression is already awaited / in an async flow.
- Building replacements that introduce `await` and propagate `async` to the containing member.

**Rationale:**

- The C# 5.0 rules are variations on the same theme: “replace blocking call with awaitable call and make caller async”.
- Centralizing this logic reduces subtle inconsistencies across rules.

**Alternatives considered:**

- Duplicating logic per rule: rejected due to maintenance cost and risk of divergent behavior.

### 3) Conservative code-fix eligibility rules

**Decision:** Only offer code fixes when the transformation is deterministic and does not change observable behavior beyond the intended async conversion.

Rule-specific guidance:

- `Thread.Sleep(...)` → `await Task.Delay(...)`:
  - Offer fix only when the containing context can be made async and the call is not in a context where `await` is illegal.
  - Prefer to preserve the argument expression as-is.
- `Task<T>.Result` / `Task.Wait()`:
  - Offer fix when the expression is used in a way that maps cleanly to `await` (e.g., `var x = task.Result;` → `var x = await task;`).
  - Avoid fixes when `.Result` is used in complex expressions where introducing `await` would require significant rewriting.
- `Task.WaitAny(...)` / `Task.WaitAll(...)`:
  - Offer fix only for the simplest patterns where return values are not used (or can be mapped safely).
  - Default to diagnostic-only if the call’s result is used, if overloads with timeouts/cancellation are involved, or if the call is inside `lock`/`catch`/`finally` patterns where `await` may be problematic.

**Rationale:**

- The analyzer should be trustworthy; a conservative fix policy reduces the chance of introducing subtle bugs.

**Alternatives considered:**

- Aggressive fixes with best-effort rewriting: rejected due to high risk and increased complexity.

### 4) Tests derived from original smoke suite, mapped to new test harness

**Decision:** Recreate tests in `Sharpen.Analyzer.Tests` by porting the original “CanBe…” and “CannotBe…” smoke files into the new test patterns.

**Rationale:**

- The smoke suite encodes the intended behavior and edge cases.
- Using the same scenarios provides regression protection and confidence in parity.

**Alternatives considered:**

- Writing new tests from scratch: rejected because it risks missing edge cases already captured in the smoke suite.

## Risks / Trade-offs

- **Risk:** Some transformations require non-trivial semantic reasoning (e.g., `WaitAny` return value usage).
  → **Mitigation:** Keep those cases diagnostic-only; add tests that assert “no code fix offered”.

- **Risk:** Introducing `await` can require cascading `async` changes up the call chain.
  → **Mitigation:** Limit fixes to cases where the containing member can be made async locally (similar to the existing await-equivalent rule). If propagation is required beyond the current member, do not offer a fix.

- **Risk:** `await` is illegal in certain contexts (e.g., `lock`, `unsafe` fixed statements, some expression-bodied members depending on rewrite).
  → **Mitigation:** Centralize “await legality” checks in shared utilities and cover with negative tests.

- **Risk:** Adding `await` may change exception timing/aggregation compared to blocking waits.
  → **Mitigation:** Document this as an inherent trade-off; only offer fixes where the original Sharpen behavior did so, and keep fixes conservative.

- **Risk:** Divergence from existing `await-equivalent-async-method` infrastructure.
  → **Mitigation:** Prefer reuse; if new utilities are introduced, refactor the existing rule to use them only if it does not change behavior.
