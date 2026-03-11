## Context

Sharpen.Analyzer provides analyzers that report diagnostics and fix providers that offer code actions. Some fixes are only valid when the target syntax/semantic context meets specific preconditions (e.g., nullability flow, language version, symbol shape, or syntactic form).

Today, “is it safe to report/offer this?” is either:
- embedded inside analyzers/fix providers inconsistently,
- duplicated across analyzer and fix provider paths, or
- missing, leading to fragile code actions and noisy diagnostics.

We already have the concept of a safety layer, but we need to make the relationship between a fix provider and its safety logic explicit and enforceable.

Constraints:
- Safety must run early enough to prevent diagnostics from being reported when the fix would be invalid.
- The same safety decision must gate both diagnostic reporting and code action offering.
- The system must be easy to extend when adding new fix providers.

## Goals / Non-Goals

**Goals:**
- Establish a one-to-one mapping between each fix provider and a dedicated safety checker.
- Ensure the safety checker runs:
  - before the analyzer reports the diagnostic, and
  - before the fix provider offers a code action.
- Provide a modular, maintainable structure:
  - safety checkers are small, focused, and testable,
  - mapping is centralized and validated.
- Provide a clear integration flow example (NullCheckAnalyzer + NullCheckSafetyChecker + NullCheckFixProvider).

**Non-Goals:**
- Implementing new fixes or changing the semantics of existing fixes beyond adding the safety gate.
- Building a general-purpose policy engine; the intent is a simple, explicit mapping.
- Optimizing for micro-performance at the expense of clarity (basic caching can be added later if needed).

## Decisions

### 1) One-to-one mapping: FixProvider ↔ SafetyChecker

**Decision:** Each fix provider has exactly one corresponding safety checker type.

**Rationale:**
- Enforces ownership: the safety logic for a fix is not scattered.
- Prevents drift: analyzer and fix provider cannot accidentally use different safety criteria.
- Improves discoverability: “where is the safety logic for this fix?” has a single answer.

**Alternatives considered:**
- *Many-to-one (shared checker for multiple fixes):* reduces code but increases coupling and makes changes riskier.
- *One-to-many (multiple checkers per fix):* flexible but complicates composition, ordering, and debugging.

### 2) Safety runs before diagnostic emission and before code action offering

**Decision:** The analyzer pipeline must consult the safety checker before reporting the diagnostic; the fix provider must consult the same checker before registering code actions.

**Rationale:**
- Avoids “diagnostic with no valid fix” scenarios.
- Avoids offering code actions that will fail or produce invalid code.
- Makes behavior consistent across IDE experiences.

**Alternatives considered:**
- *Only gate code actions:* still leaves noisy diagnostics and inconsistent UX.
- *Only gate diagnostics:* still allows code actions to appear in contexts where they cannot apply.

### 3) Central registration + validation

**Decision:** Maintain a single mapping registry (e.g., a dictionary keyed by fix provider type) and validate at startup/test time that:
- every fix provider is mapped,
- no fix provider maps to multiple checkers,
- (optionally) every checker is referenced.

**Rationale:**
- Makes the system self-auditing.
- Enables a simple “mapping completeness” test.

**Alternatives considered:**
- *Attribute-based discovery:* convenient but less explicit and harder to validate across assemblies.
- *Convention-only (naming):* brittle and not enforceable.

### 4) Foldering and base types for maintainability

**Decision:** Introduce/standardize a `Safety/` folder with base types and per-fix checkers.

**Rationale:**
- Keeps safety concerns separated from analyzers and fix providers.
- Encourages small, composable checkers.
- Improves onboarding: new contributors can follow a predictable structure.

## Risks / Trade-offs

- **[Risk] Increased boilerplate (one checker per fix)** → **Mitigation:** keep checker interface minimal; provide shared helpers for common patterns.
- **[Risk] Mapping registry becomes a merge hotspot** → **Mitigation:** keep mapping table small and sorted; consider partial registries per feature area if it grows.
- **[Risk] Safety checks require semantic model and may be expensive** → **Mitigation:** allow checkers to short-circuit on syntax-only checks; add caching later if profiling shows need.
- **[Risk] Behavior change: fewer diagnostics in edge cases** → **Mitigation:** document the gating behavior; add tests to lock expected outcomes.

## Migration Plan

1. Create/standardize safety base types and folder structure.
2. Implement safety checkers for the targeted fix providers.
3. Add the central mapping registry and validation tests.
4. Update analyzer pipeline to consult the mapped safety checker before reporting diagnostics.
5. Update fix providers to consult the same checker before offering code actions.
6. Add integration tests demonstrating the NullCheck flow end-to-end.
7. Update documentation for adding new fix provider + safety checker pairs.

## Open Questions

- Should safety evaluation be cached per document/compilation to avoid repeated semantic queries?
- Should the mapping be keyed by diagnostic ID, fix provider type, or both (for providers that handle multiple diagnostics)?
- Do we need a “default deny” behavior (no mapping → no diagnostic/action) or “default allow” with warnings? (Proposal assumes default deny for safety.)
