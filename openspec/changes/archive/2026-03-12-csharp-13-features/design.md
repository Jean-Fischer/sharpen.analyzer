## Context

Sharpen.Analyzer provides Roslyn analyzers and code fixes to help modernize C# codebases across language versions. The project already organizes rules and fix providers by C# version (e.g., C# 10) and has an emerging “fix provider safety checker” layer to prevent unsafe automated transformations.

C# 13 introduces a mix of:

- Pure compiler behavior changes (low ROI for analyzers)
- New syntax sugar (good candidates for suggestion + safe fix)
- New BCL types / attributes (good candidates for guidance; fixes require strong safety constraints)
- New partial member forms (refactoring-like transformations)

This change focuses on the subset that can be detected reliably with Roslyn semantic analysis and, where feasible, fixed safely with a dedicated safety checker.

Constraints:

- Avoid breaking changes: any fix that changes public API surface must be opt-in and generally avoided.
- Prefer deterministic, local transformations; solution-wide changes are allowed only when the change is non-public and all call sites are in-solution.
- Safety checkers must be conservative: if analysis cannot prove safety, do not offer the fix.

## Goals / Non-Goals

**Goals:**

- Add a C# 13 rule set with analyzers for the most valuable features:
  - `params` collections (restricted to non-public APIs for fixes)
  - `^` from-end indices in object/collection initializers (safe patterns)
  - `\e` escape sequence replacement (safe patterns)
  - `System.Threading.Lock` migration suggestions (safe patterns)
  - Partial properties/indexers refactoring (where feasible)
  - Guidance-only analyzers for `allows ref struct` and `OverloadResolutionPriorityAttribute`
- Provide fix providers for the “safe subset” of each feature.
- Provide safety checkers for each fix provider.
- Provide unit tests for analyzers and fixes (positive + negative).
- Update documentation to describe the new rules and safety constraints.

**Non-Goals:**

- Implement analyzers for compiler-only behavior changes with low actionable value (e.g., method group natural type improvements).
- Provide automatic fixes for ref/unsafe in iterators/async methods (too easy to violate ref-safety rules).
- Provide automatic fixes for library design choices that require human judgment (e.g., interface implementation by ref structs).

## Decisions

1. **Introduce a C# 13 rule namespace/folder aligned with existing versioned structure**

   - Decision: Add analyzers/fix providers under a new C# 13 grouping consistent with existing C# 10/11/12 patterns.
   - Rationale: Keeps discoverability and avoids mixing rules across versions.
   - Alternatives:
     - Put rules in a “Modern C#” bucket: rejected because the project already uses versioned organization.

2. **Treat “signature-changing” fixes as restricted refactorings**

   - Decision: For `params` collections, only offer a fix when the method is non-public and all call sites can be updated in the current solution.
   - Rationale: Changing `params T[]` to `params ReadOnlySpan<T>` (or similar) is a breaking change for public APIs.
   - Alternatives:
     - Offer fix for public APIs: rejected due to binary compatibility and downstream callers.

3. **Adopt a “safety checker first” policy for any non-trivial fix**

   - Decision: Every new fix provider must have a corresponding safety checker that can veto the fix.
   - Rationale: C# 13 features often have subtle semantic constraints; conservative gating reduces false positives and unsafe edits.
   - Alternatives:
     - Rely on analyzer-only heuristics: rejected; fix providers need stronger guarantees.

4. **Prefer minimal, syntax-preserving transformations**

   - Decision: For `\e` and `^` index suggestions, only transform when the replacement is a direct syntactic substitution with identical semantics.
   - Rationale: These are “sugar” features; the fix should be predictable and low-risk.

5. **Partial properties/indexers as refactoring-style code actions**

   - Decision: Implement partial property/indexer support as a refactoring-like code action (not a high-severity diagnostic), and only when the transformation can be proven to preserve signature and accessibility.
   - Rationale: These changes can be invasive (file splitting, generated code patterns) and are best offered as an explicit refactoring.

6. **Guidance-only analyzers for design-heavy features**

   - Decision: For `allows ref struct` and `OverloadResolutionPriorityAttribute`, start with analysis-only diagnostics and optionally provide a “requires review” code action.
   - Rationale: These are library design decisions; automated changes can be harmful without context.

## Risks / Trade-offs

- **[Risk] False positives for `params` collections** → **Mitigation**: require semantic proof that the method body does not rely on array-only semantics (e.g., `Length`, indexing, `Array.*`, `GetLength`, `Rank`, `CopyTo`) and that the symbol is non-public.
- **[Risk] Solution-wide fix complexity** → **Mitigation**: use Roslyn solution edits (find references + update invocation arguments) and keep the supported transformation set small.
- **[Risk] `System.Threading.Lock` availability depends on target framework** → **Mitigation**: only offer the fix when the compilation references the type (or when the project’s target framework is known to include it).
- **[Risk] Partial property/indexer refactoring can break generated code conventions** → **Mitigation**: keep the first iteration limited to same-type partial declarations within the same project and avoid cross-file moves unless explicitly requested by the user action.
- **[Trade-off] Some C# 13 features will remain documentation-only** → Acceptable to keep ROI high and avoid fragile analyzers.
