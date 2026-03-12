## Context

Sharpen is a Roslyn-based analyzer + code-fix suite that provides “modernize to newer C#” suggestions with a strong emphasis on safe, reviewable transformations. The project already has:

- A rule catalog with diagnostic IDs and documentation.
- A fix-provider layer with “safety checkers” to ensure code actions are only offered when transformations are semantically safe.
- A test suite validating diagnostics and code fixes.

This change adds a curated set of C# 14 rules, and also migrates rule IDs from the legacy `EDRxxxx` prefix to `SHARPENxxxx` (starting at `SHARPEN001`, continuing from the current highest `SHARPEN` ID in the repo).

Constraints:

- Analyzers must run on projects targeting older language versions without crashing. Any syntax-node usage must be guarded (feature-gated) and/or rely on semantic patterns that exist in older syntax trees.
- Code fixes must be conservative and must integrate with the existing safety-checker pipeline.
- Rule IDs must remain stable once introduced; the migration must be done in a single coordinated sweep to avoid mixed prefixes.

## Goals / Non-Goals

**Goals:**

- Add analyzers for the selected C# 14 features, each with:
  - Diagnostic rule definition (new `SHARPENxxx` ID).
  - Safety checker (even for “no fix” rules, to document why no fix is offered).
  - Code fix provider when feasible.
  - Unit tests for diagnostics and fixes.
  - Documentation updates.
- Implement a deterministic rule-ID migration from `EDR` → `SHARPEN`:
  - Rename `EDR001` → `SHARPEN001`.
  - Increment/assign subsequent `SHARPEN` IDs based on the existing project’s current highest `SHARPEN` ID.
  - Update all references (diagnostic descriptors, docs, tests, sample code, any mapping tables).

**Non-Goals:**

- Implementing risky, whole-solution refactors (e.g., changing public APIs from `T[]` to `ReadOnlySpan<T>` across projects) as automatic fixes.
- Providing code fixes for generator-coupled features (partial constructors/events) or semantics-heavy operator generation (compound assignment operators).
- Enforcing C# 14 adoption; these are suggestions/info-level rules unless a clear correctness issue exists.

## Decisions

1) Rule ID strategy: `SHARPEN` prefix with sequential numbering

- Decision: All new C# 14 rules use `SHARPENxxx` IDs. The migration replaces existing `EDRxxx` IDs with `SHARPENxxx` IDs, starting at `SHARPEN001` and continuing from the current highest `SHARPEN` ID.
- Rationale: A single consistent prefix improves discoverability and branding, and avoids maintaining two parallel ID namespaces.
- Alternative: Keep `EDR` for legacy rules and introduce `SHARPEN` only for new rules.
  - Rejected because it fragments documentation and user configuration (EditorConfig) across prefixes.

2) “Spec-driven” structure: one spec per capability/rule family

- Decision: Create one spec per capability listed in the proposal (mostly one per feature), each describing:
  - Diagnostic intent and severity.
  - Safety constraints.
  - Code fix behavior (if any).
  - Examples and non-examples.
- Rationale: Keeps requirements explicit and testable; aligns with existing OpenSpec workflow.

3) Safety-first code fixes

- Decision: Only offer code fixes when the safety checker can prove the transformation is safe using Roslyn semantic analysis.
- Rationale: Avoids false positives and broken refactors; matches Sharpen’s existing philosophy.
- Alternative: Offer broader fixes with warnings.
  - Rejected because it increases risk and reduces trust in automated fixes.

4) Feature gating and compatibility

- Decision: Analyzers should detect patterns using semantic model and existing syntax constructs where possible, and only use C# 14 syntax nodes behind guards (e.g., checking `LanguageVersion` or parsing options).
- Rationale: Analyzer packages are often used in mixed-language-version solutions; they must not throw on older syntax trees.

## Risks / Trade-offs

- [Risk] Rule ID migration breaks existing `.editorconfig` suppressions and documentation links.
  → Mitigation: Provide a migration note in docs, and (if the project supports it) consider temporary “alias” diagnostics or a mapping table in documentation.

- [Risk] Over-eager code fixes change semantics (especially for extension members, span conversions, and null-conditional assignment).
  → Mitigation: Conservative analyzers + safety checkers; only fix the simplest patterns; add “do not fix” tests.

- [Risk] C# 14 syntax not available in the current build/test toolchain.
  → Mitigation: Ensure the repo targets an SDK/compiler that can parse C# 14; if not, implement analyzers using semantic patterns and keep fixes behind language-version checks until the toolchain is updated.

- [Risk] Span-related suggestions can be subtly wrong due to lifetime/escape rules.
  → Mitigation: Limit to removing redundant conversions in contexts where the semantic model confirms the conversion is identity-equivalent and the span does not escape.
