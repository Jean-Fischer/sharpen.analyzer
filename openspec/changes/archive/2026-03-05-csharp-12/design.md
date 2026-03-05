## Context

Sharpen.Analyzer provides Roslyn analyzers and code fixes that help developers modernize C# code by suggesting newer language features. The repository already contains similar “use X” analyzers (e.g., records, init-only setters, top-level statements) with a consistent pattern for:

- registering rules/diagnostic IDs
- implementing analyzers that detect a refactoring opportunity
- implementing code fix providers that rewrite syntax trees
- unit tests that validate both diagnostics and fixes

This change adds a C# 12-focused set of analyzers and fixes:

- primary constructors
- collection expressions
- default lambda parameters
- inline arrays

Constraints / assumptions:

- The analyzer/test infrastructure must compile with a Roslyn version that understands C# 12 syntax.
- Each rule should be conservative: only offer a fix when the transformation is semantics-preserving.
- Code fixes should preserve trivia/formatting as much as possible and rely on existing helper utilities where available.

## Goals / Non-Goals

**Goals:**

- Add four new rules (analyzer + code fix) corresponding to the capabilities in the proposal:
  - `use-primary-constructors`
  - `use-collection-expressions`
  - `use-default-lambda-parameters`
  - `use-inline-arrays`
- Ensure rules are registered and surfaced consistently (diagnostic descriptors, rule list, fix provider export).
- Provide unit tests for each rule covering:
  - positive cases (diagnostic + fix)
  - negative cases (no diagnostic)
  - edge cases where the fix must not be offered

**Non-Goals:**

- Implementing every possible C# 12 transformation variant; initial scope targets the common, unambiguous patterns.
- Large-scale formatting changes or style enforcement beyond the minimal syntax rewrite.
- Introducing new public APIs beyond what is required for analyzers/fixes.

## Decisions

1) Rule structure and naming

- Decision: Follow existing “UseX” naming conventions for analyzer and code fix provider types, and add new rule IDs in the central rule registry.
- Rationale: Consistency with existing rules reduces maintenance cost and makes the new rules discoverable.
- Alternatives:
  - Create a single “CSharp12ModernizationAnalyzer” with multiple diagnostics. Rejected: harder to test and maintain; inconsistent with existing structure.

2) Primary constructors detection strategy

- Decision: Trigger only when a type has exactly one instance constructor whose body is a sequence of simple assignments from parameters to members, and no other constructor logic.
- Rationale: This is the safest subset and matches the intended “constructor only assigns parameters” rule.
- Notes / constraints:
  - Require that each parameter is used exactly once in an assignment.
  - Allow assignments to auto-properties via backing assignment patterns only if they are trivially convertible.
  - Do not trigger if:
    - there are multiple constructors
    - the constructor calls `this(...)` / `base(...)` with arguments that are not a direct parameter pass-through
    - there are side effects (method calls, conditionals, loops, try/catch)
    - assignments target static members
- Fix approach:
  - Convert the type declaration to include a primary constructor parameter list.
  - Convert assigned members to get-only auto-properties (or fields) initialized from the corresponding parameter.
  - Preserve accessibility and existing member names.
- Alternatives:
  - Offer fix even when additional constructor logic exists by splitting logic into an instance initializer. Rejected: higher risk and more complex.

3) Collection expressions detection strategy

- Decision: Support the most common initializer forms first:
  - array creation with initializer: `new T[] { ... }` and `new[] { ... }`
  - collection initializer for `List<T>` and other known collection types where the target type can be constructed from a collection expression
- Rationale: C# 12 collection expressions are broadly applicable, but correctness depends on target typing and available constructors/collection builders.
- Fix approach:
  - Replace initializer with `[...]` while preserving element expressions.
  - Prefer using target typing when possible (e.g., `List<int> x = [1,2,3];`).
- Guardrails:
  - Do not trigger when the initializer contains spread elements or complex patterns that would change semantics.
  - Ensure the language version supports collection expressions in the compilation.
- Alternatives:
  - Only support arrays initially. Acceptable fallback if Roslyn APIs make general collection support too complex.

4) Default lambda parameters detection strategy

- Decision: Detect lambdas where parameters are explicitly typed and have default values expressed in a way compatible with C# 12 default parameter syntax.
- Rationale: The feature is syntactic; the analyzer should only suggest when the rewrite is straightforward.
- Fix approach:
  - Rewrite the lambda parameter list to include `= <default>`.
- Guardrails:
  - Do not trigger for implicitly-typed lambdas if the rewrite would require adding types.
  - Do not trigger if default values depend on non-constant expressions that are not allowed.

5) Inline arrays detection strategy

- Decision: Detect structs that represent fixed-size buffers using the conventional pattern:
  - a `struct` with a single field named like `_element0` (or equivalent) and an implied length N
  - or a sequence of fields `_element0.._element{N-1}`
- Rationale: Inline arrays require a very specific layout; we should only suggest when the pattern is clearly intended as a fixed-size buffer.
- Fix approach:
  - Add `[System.Runtime.CompilerServices.InlineArray(N)]` attribute to the struct.
  - Replace the field set with a single field `_element0` of the element type (if not already in that form).
- Guardrails:
  - Do not trigger if the struct has additional fields, methods, or explicit layout attributes that could conflict.

6) Provider wiring / registration

- Decision: Add new diagnostic descriptors and ensure each code fix provider is exported and associated with the correct diagnostic ID(s).
- Rationale: The proposal explicitly calls out “fix provider (if applicable)” issues; we should validate the existing registration pattern and extend it.
- Alternatives:
  - Central “mega” code fix provider. Rejected: less modular.

## Risks / Trade-offs

- [Roslyn/C# 12 support mismatch] → Mitigation: update analyzer/test compilation settings and/or package references so parsing/binding C# 12 syntax is supported.
- [False positives for primary constructors] → Mitigation: keep detection conservative; require a strict assignment-only constructor body.
- [Collection expressions semantics differences] → Mitigation: start with arrays and well-known target-typed cases; add semantic checks before offering fixes.
- [Inline array pattern ambiguity] → Mitigation: only trigger on canonical buffer patterns; avoid structs with extra members.
