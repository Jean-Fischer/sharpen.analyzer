## Context

The legacy Sharpen project contains a set of analyzers targeting C# 8. This repository already contains analyzers for earlier language versions (e.g., C# 5) and has an established structure for analyzers, code fixes, and tests.

The goal of this change is to migrate the C# 8 analyzers (and any associated code fixes and tests) from the old project into the new project, preserving diagnostic behavior and developer experience.

Constraints / assumptions:
- The new project’s conventions (folder layout, base test infrastructure, naming, packaging) should be followed.
- Diagnostic IDs, titles, messages, severities, and categories should remain stable unless there is a strong reason to change (changing them is effectively a breaking change for users).
- Some legacy analyzers may rely on older Roslyn APIs; the migration may require updating implementation details to match the Roslyn version used by this repository.

## Goals / Non-Goals

**Goals:**
- Identify all analyzers in the legacy project that are intended for C# 8.
- Port each analyzer into the new project under the established analyzer structure.
- Port associated code fixes (when present in the legacy project) and any required shared helper code.
- Port and/or rewrite unit tests so that each migrated analyzer has coverage in the new test suite.
- Ensure behavior parity with the legacy project (diagnostic triggers, locations, messages, and code fix output).
- Ensure the solution builds and all tests pass.

**Non-Goals:**
- Redesigning analyzer rules, changing diagnostic IDs, or altering user-facing messages (unless required for correctness).
- Large-scale refactors unrelated to the migration.
- Adding new analyzers beyond what exists in the legacy C# 8 set.

## Decisions

1) **Inventory-first migration**
- Decision: Start by enumerating the legacy C# 8 analyzers and creating a mapping list (legacy type → new type/file path, diagnostic ID, and whether a code fix exists).
- Rationale: Prevents missing analyzers and provides a checklist for completion.
- Alternatives:
  - “Port as you find them”: higher risk of omissions and inconsistent naming.

2) **Preserve diagnostic contract**
- Decision: Keep diagnostic IDs, titles, messages, and default severities the same as the legacy project.
- Rationale: Users may depend on IDs for suppressions, baselines, and CI rules.
- Alternatives:
  - Renumber/rename diagnostics: would be a breaking change and complicate migration.

3) **Adopt new project conventions for structure**
- Decision: Place analyzers under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/) and code fixes under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/), using existing patterns in this repository.
- Rationale: Consistency improves maintainability and discoverability.
- Alternatives:
  - Keep legacy folder layout: would diverge from repository conventions.

4) **Test parity as the acceptance mechanism**
- Decision: For each migrated analyzer, port existing tests when available; otherwise, create new tests that cover the same scenarios as the legacy behavior.
- Rationale: Tests are the most reliable way to ensure parity and prevent regressions.
- Alternatives:
  - Manual verification only: error-prone and not repeatable.

5) **Roslyn API compatibility strategy**
- Decision: Prefer minimal code changes to compile against the current Roslyn version; if an API is obsolete/removed, refactor locally within the analyzer/code fix while keeping external behavior identical.
- Rationale: Keeps the migration focused while ensuring compatibility.
- Alternatives:
  - Pin Roslyn to legacy version: likely undesirable and may conflict with the rest of the repository.

## Risks / Trade-offs

- **[Risk] Missing analyzers during migration** → Mitigation: Create an explicit inventory list and track completion in tasks.
- **[Risk] Behavior drift due to Roslyn API differences** → Mitigation: Port tests and add new tests for edge cases; compare diagnostic output with legacy behavior.
- **[Risk] Shared helper code duplication** → Mitigation: Consolidate shared helpers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/) and reuse across analyzers.
- **[Trade-off] Strict parity vs. adopting new conventions** → Mitigation: Keep user-facing behavior identical while allowing internal refactors needed for compatibility and conventions.