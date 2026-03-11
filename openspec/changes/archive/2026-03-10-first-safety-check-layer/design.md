## Context

Sharpen.Analyzer fix providers currently rely on pattern matching to identify candidate code locations and then proceed to compute/apply code fixes. Some fixes require additional semantic preconditions (e.g., symbol resolution, type constraints, language version constraints, trivia/formatting constraints) that are not always guaranteed by the pattern match alone.

When these preconditions are not met, a fix can:
- produce incorrect edits,
- throw during fix computation,
- or apply a “technically valid” edit that changes meaning.

We already have work around safety checking in the fix-provider area, but we need an explicit, consistent “first safety check” gate that runs immediately after a match is found and before any fix is generated.

Constraints:
- Must be cheap enough to run for every match candidate.
- Must be deterministic: same input → same pass/fail.
- Must be composable: multiple checks can be applied.
- Must be observable in tests (and optionally in logs) without requiring edits.

## Goals / Non-Goals

**Goals:**
- Add a first-pass safety-check stage between pattern matching and fix generation/application.
- Define a small contract for safety checks:
  - input context (document, semantic model, match result, cancellation token),
  - output (pass/fail + reason code/message).
- Ensure the pipeline short-circuits on failure (no edits produced).
- Make it easy for fix providers to register checks and for tests to assert “match found but not safe”.

**Non-Goals:**
- Rewriting all existing fix providers in this change.
- Introducing a new diagnostic reporting UX (beyond optional internal logging/test visibility).
- Adding expensive whole-solution analysis in the first-pass checks.

## Decisions

1) **Introduce an explicit gate API between match and fix**
- Decision: Add a dedicated “first safety check” step that is invoked after a match is produced and before fix computation.
- Rationale: This makes the pipeline structure explicit and prevents ad-hoc checks scattered across fix providers.
- Alternatives:
  - Keep checks inside each fix provider’s fix computation: rejected because it is inconsistent and harder to test.
  - Add checks inside pattern matching: rejected because many checks require semantic context and are conceptually separate from matching.

2) **Standardize check results**
- Decision: Safety checks return a structured result (e.g., `Safe`/`Unsafe`) with a reason identifier and optional message.
- Rationale: Enables consistent short-circuiting and test assertions.
- Alternatives:
  - Boolean only: rejected because it loses the reason and makes debugging/tests harder.

3) **Composable checks with predictable ordering**
- Decision: Allow multiple checks to run in a defined order (e.g., registration order) and stop at first failure.
- Rationale: Keeps runtime cost low and makes behavior deterministic.
- Alternatives:
  - Run all checks and aggregate: rejected for cost and because first failure is usually sufficient.

4) **Integration point: fix-provider pipeline**
- Decision: The gate lives in the fix-provider orchestration layer (the component that currently bridges match → fix).
- Rationale: Centralizes behavior and avoids duplicating logic across providers.

## Risks / Trade-offs

- **[Risk] Added complexity in the fix pipeline** → Mitigation: keep the API minimal; provide a default “no checks” path.
- **[Risk] Performance overhead** → Mitigation: first-pass checks must be cheap; stop at first failure; avoid allocations where possible.
- **[Risk] Inconsistent adoption across fix providers** → Mitigation: provide a clear registration mechanism and add tests for at least one representative provider.
- **[Risk] Ambiguity between “match failed” vs “match succeeded but unsafe”** → Mitigation: represent these as distinct outcomes in the pipeline and in tests.
