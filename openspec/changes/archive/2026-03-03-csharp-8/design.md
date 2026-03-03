## Context

This change ports the C# 7.1 feature set from the upstream/original Sharpen repository into this repository (Sharpen.Analyzer), following the same migration approach already used for earlier language-version migrations.

The upstream/original Sharpen repository is included in this workspace under [`original-sharpen/`](original-sharpen/). The goal is to copy/translate the relevant C# 7.1 analyzers, code fixes, tests, and samples into the new project layout and naming conventions.

Known upstream C# 7.1 feature coverage (initial scope):
- **Default literal / default expressions** (C# 7.1): suggestions under `SharpenSuggestions/CSharp71/DefaultExpressions/*`.

Constraints:
- Keep the public surface area consistent with existing analyzers (naming, categories, severity defaults).
- Prefer reusing existing shared helpers in this repo; only port upstream helpers when necessary.
- Ensure all new rules are covered by tests and have sample snippets.

## Goals / Non-Goals

**Goals:**
- Identify the set of C# 7.1-related analyzers/code fixes in the upstream project and migrate them into:
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/)
- Port/author unit tests for each migrated rule in:
  - [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/)
- Port/author sample snippets into the sample project under a new `csharp71` folder, mirroring the existing sample structure.
- Ensure the migrated analyzers are registered and packaged consistently (diagnostic descriptors, supported diagnostics, etc.).

**Non-Goals:**
- Rewriting or redesigning existing analyzers.
- Introducing new C# 7.1 rules that do not exist upstream (unless required to complete the migration due to missing dependencies).
- Large-scale refactors unrelated to the migration.

## Decisions

1) **Migration strategy: port rule-by-rule, keeping behavior identical**
- Decision: Migrate each upstream analyzer/code fix pair as a unit, preserving diagnostic behavior and code fix output.
- Rationale: Minimizes regressions and keeps parity with upstream.

2) **Project structure: follow existing Sharpen.Analyzer conventions**
- Decision: Place analyzers in the local Analyzers folder and code fixes in FixProvider, matching existing rules.
- Rationale: Consistency with the current project makes maintenance and discovery easier.

3) **Shared helpers: reuse local helpers first, port only when needed**
- Decision: Before porting any upstream helper, search for an equivalent helper already present in this repo.
- Rationale: Avoids duplication and reduces long-term maintenance.

4) **Tests: mirror existing test patterns and naming**
- Decision: Use the same test harness and naming conventions as the existing migrations (e.g., `*Tests.cs` per rule, code fix tests where applicable).
- Rationale: Keeps the test suite uniform and reduces friction for contributors.

5) **Samples: add a dedicated `csharp71` folder**
- Decision: Add a new `csharp71` folder in the sample project and port upstream samples into it.
- Rationale: Keeps samples organized by language version.

## Risks / Trade-offs

- **[Risk] Upstream rules depend on helpers not present in this repo** → Mitigation: port the minimal required helper(s) into `Common/` and add unit tests around edge cases.
- **[Risk] Diagnostic IDs or titles conflict with existing rules** → Mitigation: keep upstream IDs if they are already used in this repo; otherwise map to the existing ID scheme and document the mapping.
- **[Risk] C# 7.1 syntax requires newer Roslyn APIs than currently referenced** → Mitigation: verify current Microsoft.CodeAnalysis package versions; if upgrade is required, do it as a separate, explicit step and run the full test suite.
- **[Trade-off] Strict parity vs. idiomatic local code**: prefer parity for behavior; allow small refactors only when required for compilation or to match local abstractions.
