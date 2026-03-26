## Context

The repository contains many Roslyn code fix providers and a growing test suite that validates each fix using helpers like [`VerifyCS.VerifyCodeFixAsync()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure:1). These tests typically assert a single transformation from “before” to “after”.

A common failure mode for code fixes is non-idempotency: applying the fix once produces code that still matches the analyzer/fix pattern, so a second application changes the code again. This can lead to cascading replacements, unstable formatting/syntax rewrites, and poor user experience in IDEs.

This change adds a cross-cutting test capability (shared helper + adoption across many test files) but does not require any production code changes.

## Goals / Non-Goals

**Goals:**

- Provide a single, reusable helper to assert code fix idempotency (apply fix twice → identical output).
- Make it easy to adopt in existing tests with minimal churn.
- Ensure the helper works with the current verification infrastructure (single-fix and “fix all” patterns where applicable).
- Enable incremental rollout: start with a subset of fix providers, then expand.

**Non-Goals:**

- Refactor the analyzer/fix provider architecture.
- Guarantee semantic equivalence beyond textual equality (the idempotency assertion is intentionally strict and text-based).
- Automatically fix non-idempotent providers as part of this change (those will be follow-up fixes once failures are exposed).

## Decisions

1. **Idempotency is asserted via text equality of the fixed output**

   - **Decision:** Compare the full fixed source text after first and second application.
   - **Rationale:** This matches how existing tests validate fixes (string-based before/after) and catches formatting/syntax instability.
   - **Alternatives considered:**
     - Compare syntax trees ignoring trivia/formatting. Rejected because many “unstable” fixes manifest as trivia changes and should be caught.

2. **Implement as a test-only extension/helper in the existing test infrastructure**

   - **Decision:** Add a helper (e.g., `VerifyIdempotentCodeFixAsync`) under the test infrastructure folder, close to existing verification helpers.
   - **Rationale:** Keeps production assemblies unchanged and makes adoption straightforward.
   - **Alternatives considered:**
     - Add idempotency checks inside every test manually. Rejected due to duplication and inconsistent usage.

3. **Adoption strategy: wrap existing tests rather than rewriting them**

   - **Decision:** Provide helper overloads that accept the same inputs as current tests (before/after strings, optional code fix index / equivalence key) and internally run the second pass.
   - **Rationale:** Minimizes changes across many test files and reduces risk of introducing test regressions.
   - **Alternatives considered:**
     - Replace all tests with a new pattern. Rejected as too disruptive.

4. **Scope includes “single fix” idempotency first; expand to “fix all” where supported**

   - **Decision:** Start by ensuring the helper supports the common single-document, single-fix flow; add optional support for fix-all verification if the infrastructure exposes it.
   - **Rationale:** Immediate value with low complexity; fix-all idempotency can be layered on.

## Risks / Trade-offs

- **[Risk] False positives due to formatting differences between passes** → **Mitigation:** Treat as a real issue; if formatting is unstable, the fix is not deterministic. If needed, ensure the helper uses the same formatting options on both passes.
- **[Risk] Some fixes intentionally require multiple passes** → **Mitigation:** Document an escape hatch (explicit opt-out) but require justification; prefer making fixes converge in one pass.
- **[Risk] Increased test runtime** → **Mitigation:** Apply idempotency checks incrementally and only to code fix tests (not analyzer-only tests). Consider batching or limiting to representative cases if runtime becomes an issue.
- **[Trade-off] Text equality is strict** → This is intentional to catch unstable rewrites; it may require small adjustments in fix providers to become deterministic.