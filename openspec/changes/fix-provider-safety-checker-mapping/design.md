## Context

Sharpen.Analyzer has a growing set of code fix providers and a parallel set of “safety checkers” intended to validate whether a fix is safe to apply in a given context. The current architecture does not strictly enforce that every fix provider has exactly one corresponding safety checker, nor does it provide a single, canonical integration flow that always runs the correct safety checker before applying a fix.

This creates two recurring risks:

- A fix provider can be added without a safety checker (or with an incorrectly wired checker), leading to unsafe fixes being offered/applied.
- Safety checkers can exist without a clear ownership relationship to a fix provider, making the system harder to reason about and maintain.

Constraints / assumptions:

- The mapping must be discoverable at runtime (for the integration flow) and verifiable at build/test time.
- The mapping should be explicit and easy to extend when adding new fix providers.
- The system should fail fast (or at least surface clear diagnostics) when the mapping is incomplete or ambiguous.

## Goals / Non-Goals

**Goals:**

- Enforce a one-to-one relationship between fix providers and safety checkers.
- Provide a single integration flow that:
  1) discovers fix providers,
  2) resolves the mapped safety checker,
  3) runs the safety checker,
  4) only then applies the fix.
- Provide validation that:
  - every fix provider has a mapped safety checker,
  - no safety checker is mapped to multiple fix providers,
  - the mapping is deterministic and does not depend on ordering.
- Make it straightforward to add a new fix provider + safety checker pair.

**Non-Goals:**

- Redesigning the internal logic of individual safety checkers.
- Changing the semantics of existing fixes beyond requiring the safety check to pass.
- Introducing a new public API surface outside the analyzer/fix infrastructure (unless required by existing extension points).

## Decisions

### 1) Represent the mapping as an explicit registry

**Decision:** Introduce a single registry that defines the mapping between a fix provider type (or identifier) and a safety checker type.

**Rationale:**

- Centralizes the contract and makes it auditable.
- Enables deterministic validation (one-to-one) and clear error reporting.
- Avoids “implicit” wiring via naming conventions or scattered attributes.

**Alternatives considered:**

- Attribute-based mapping on fix providers (e.g., `[SafetyChecker(typeof(...))]`).
  - Pros: mapping is colocated with the fix provider.
  - Cons: harder to validate globally; reflection/MEF discovery can hide duplicates; harder to enforce “no checker mapped twice” without a global scan.
- Convention-based mapping (e.g., naming patterns).
  - Pros: minimal boilerplate.
  - Cons: brittle, unclear ownership, and difficult to evolve.

### 2) Enforce one-to-one via validation at startup and in tests

**Decision:** Validate the mapping in two places:

- Runtime validation during initialization of the fix/safety pipeline (fail fast with a clear exception/diagnostic).
- Unit tests that assert the mapping is complete and one-to-one.

**Rationale:**

- Runtime validation protects end users and catches misconfiguration in production.
- Tests provide fast feedback for contributors and CI.

**Alternatives considered:**

- Only runtime validation: risks late discovery and harder debugging.
- Only tests: risks missing issues in non-test execution paths or packaging.

### 3) Standardize the integration flow around the mapping

**Decision:** The integration flow that applies fixes must resolve the safety checker exclusively through the mapping registry (no ad-hoc resolution).

**Rationale:**

- Ensures the one-to-one contract is actually used.
- Prevents bypassing safety checks.

**Alternatives considered:**

- Allow “fallback” safety checker resolution when mapping is missing.
  - Rejected because it undermines the enforcement goal.

### 4) Define a stable identity for fix providers and safety checkers

**Decision:** Use a stable identifier for mapping keys (preferably the fix provider type, or a unique fix-provider id if types are not stable across discovery boundaries).

**Rationale:**

- Type-based mapping is simple and strongly typed.
- If discovery uses identifiers rather than types, the mapping should align with that to avoid mismatches.

**Alternatives considered:**

- String-based keys everywhere: flexible but error-prone.

## Risks / Trade-offs

- **Risk:** Existing fix providers may not have safety checkers yet → **Mitigation:** introduce the mapping with a migration phase where missing mappings are surfaced as test failures first, then upgraded to runtime failures once coverage is complete.
- **Risk:** Multiple discovery mechanisms (MEF/reflection/manual registration) could lead to duplicate registrations → **Mitigation:** make the registry the single source of truth and have discovery feed into it rather than bypass it.
- **Trade-off:** Central registry adds a small amount of boilerplate when adding new fix providers, but improves correctness and maintainability.

## Migration Plan

1. Introduce the mapping registry and validation logic.
2. Add mapping entries for existing fix providers and their safety checkers.
3. Update the fix application pipeline to resolve safety checkers via the registry.
4. Add tests that assert one-to-one completeness.
5. Tighten validation to fail fast (if initially introduced as warnings) once the mapping is complete.

## Open Questions

- Should the mapping key be the fix provider type, a diagnostic id, or a dedicated fix-provider id?
- What is the desired failure mode in production: throw, log + skip fix, or surface a diagnostic?
- Do any fix providers intentionally share a safety checker today (if so, do we need a composite checker instead)?
