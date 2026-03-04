## Context

Sharpen.Analyzer implements Roslyn analyzers + code fixes to help modernize C# code. The existing codebase already contains patterns for:
- Single-file analyzers (e.g., “use record type”, “use top-level statements”).
- Code fix providers that rewrite syntax nodes while preserving trivia.
- Unit tests validating diagnostics and code fixes.

This change adds a set of C# 10-focused analyzers and emphasizes richer tests/samples that cover realistic multi-declaration and multi-file scenarios.

Constraints:
- The analyzers should be safe-by-default: only suggest transformations when semantics are preserved.
- Tests should include “complex but realistic” scenarios (multiple members, nested namespaces, alias/static usings, null-conditional chains, etc.).

## Goals / Non-Goals

**Goals:**
- Implement 5 new analyzers + code fixes:
  - File-scoped namespaces
  - Global using directives
  - Record structs
  - Extended property patterns
  - Constant interpolated strings
- Provide test coverage that includes:
  - Multiple declarations per file
  - Nested constructs (nested namespaces, nested property access)
  - Multi-file/project context where required (global usings)
- Keep diagnostics and fixes consistent with existing Sharpen rules (naming, severity, registration).

**Non-Goals:**
- Full semantic equivalence proof for all possible code patterns; we will scope to safe, common patterns.
- Implementing a full “project-wide refactoring” that automatically creates a new `GlobalUsings.cs` file and removes all local usings; instead, provide a per-document fix and rely on “Fix all” where supported.
- Supporting pre-C# 10 language versions for these rules.

## Decisions

1. **Rule structure: one analyzer + one code fix per capability**
   - Rationale: matches existing Sharpen patterns and keeps each feature independently testable.
   - Alternative: a single “C# 10 modernization” analyzer with multiple diagnostics. Rejected due to complexity and harder test isolation.

2. **File-scoped namespaces: only when exactly one top-level namespace exists**
   - Rationale: file-scoped namespaces are a file-level construct; multiple top-level namespaces would require splitting files.
   - Alternative: attempt to convert the first namespace only. Rejected as misleading.

3. **Global usings: implement as a multi-document analyzer**
   - Rationale: the signal is “repeated across files”, which requires project-level aggregation.
   - Approach:
     - Collect `UsingDirectiveSyntax` across documents.
     - Group by normalized text (including `static` and alias forms).
     - Report diagnostics for directives whose count >= threshold (start with 2).
   - Alternative: purely single-file heuristic (“common usings”). Rejected because it would be noisy.

4. **Record structs: conservative detection**
   - Rationale: converting to `record struct` can change generated members and equality semantics; we should only suggest when the struct is clearly a value object.
   - Heuristics (initial):
     - Public struct with only public fields OR get-only auto-properties and minimal methods.
     - No settable properties, no events, no explicit interface implementations.
   - Alternative: always suggest for any struct with 2+ fields. Rejected as too noisy.

5. **Extended property patterns: focus on common rewrites**
   - Rationale: property patterns are powerful but can be hard to apply safely.
   - Scope:
     - Rewrite `x is T t && t.Prop OP value` to `x is { Prop: OP value }`.
     - Rewrite null-conditional equality checks like `x?.A?.B == c` to `x is { A: { B: c } }` when safe.
   - Alternative: rewrite arbitrary boolean expressions. Rejected.

6. **Constant interpolated strings: split into two diagnostics**
   - Rationale: “use interpolated string” is broadly applicable, while “const interpolated string” is a narrower subset.
   - Approach:
     - Detect `string.Format` and concatenation patterns.
     - If the target is `const string` and all holes are constant, offer the const-safe rewrite.

## Risks / Trade-offs

- **[Risk] Global usings analysis may be expensive on large solutions** → Mitigation: limit to project scope, cache per compilation, and use simple grouping keys.
- **[Risk] Property pattern rewrites can subtly change null-handling** → Mitigation: only rewrite patterns with clear null-conditional semantics and add tests for null cases.
- **[Risk] Record struct suggestion may be controversial/noisy** → Mitigation: conservative heuristics and tests demonstrating intended targets.
- **[Risk] Constant interpolated strings are limited by compile-time constant rules** → Mitigation: only offer const rewrite when Roslyn constant evaluation confirms constness; otherwise offer non-const interpolated string suggestion.