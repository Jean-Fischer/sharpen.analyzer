## Context

Sharpen.Analyzer’s fix-provider implementation has grown over time and now mixes multiple responsibilities:

- Roslyn `CodeFixProvider` implementations (the “assembly” side: actual fixes, diagnostics mapping, and Roslyn wiring)
- Provider/discovery/registration concerns (the “provider” side: how fix providers are surfaced, grouped, and integrated into the safety pipeline)

This makes it harder to:

- understand which layer owns which behavior
- enforce dependency direction (provider layer should not depend on concrete fix implementations)
- evolve the safety pipeline without touching unrelated fix code

The proposal introduces a clearer split between “providers” and “assembly” responsibilities.

## Goals / Non-Goals

**Goals:**

- Define a clear layering model for fix-provider code:
  - Provider layer: discovery/registration/metadata and integration points
  - Assembly layer: concrete Roslyn `CodeFixProvider` implementations
- Ensure dependency direction is explicit and enforceable (provider layer depends on abstractions/metadata, not on concrete fix implementations).
- Keep the safety-check pipeline behavior intact while moving code.
- Minimize public API churn; if unavoidable, document and update call sites.

**Non-Goals:**

- Changing the semantics of existing fixes (no behavior changes unless required by the new structure).
- Introducing new safety-check rules beyond what is needed to keep the pipeline working.
- Large-scale renaming unrelated to the provider/assembly split.

## Decisions

1) **Introduce explicit “provider” vs “assembly” boundaries**

- Decision: Treat provider/discovery/registration as a separate layer from Roslyn `CodeFixProvider` implementations.
- Rationale: This reduces coupling and makes it possible to reason about safety-pipeline integration without pulling in concrete fix implementations.
- Alternatives considered:
  - Keep current structure and add documentation only: rejected because it does not prevent future drift.
  - Split into multiple NuGet packages: rejected for now; too heavy for an internal refactor.

2) **Organize code by responsibility (folders/namespaces) rather than by language version only**

- Decision: Keep language-version grouping for concrete fixes where it helps, but ensure the top-level split is responsibility-based.
- Rationale: The primary confusion is responsibility ownership; language version is secondary.

3) **Keep safety pipeline integration in provider layer**

- Decision: Any mapping between diagnostics/fixes and safety-checkers remains in the provider layer.
- Rationale: The safety pipeline is a cross-cutting concern and should not require referencing concrete fix implementations.

## Risks / Trade-offs

- **Risk:** Type moves/renames cause widespread churn → **Mitigation:** Use IDE-assisted rename/move, keep temporary forwarding types if needed, and update tests.
- **Risk:** Accidental dependency inversion (provider layer referencing concrete fixes) → **Mitigation:** Add project/namespace-level rules (where possible) and code review checklist.
- **Risk:** Safety pipeline breaks due to moved integration points → **Mitigation:** Add/extend integration tests around fix-provider discovery and safety-check execution.
