## Context

Sharpen.Analyzer provides Roslyn analyzers and code fixes that help developers adopt newer C# language features. The codebase already contains a pattern of “UseXxx” analyzers and corresponding code fix providers, plus language-version gating tests.

This change adds a C# 11 (2022) ruleset covering:
- Raw string literals
- Required members
- Generic math support
- Pattern matching on spans/arrays (list patterns)
- UTF-8 string literals

Constraints:
- Diagnostics must be gated by language version (C# 11) to avoid suggesting features in older projects.
- Code fixes should be conservative and preserve semantics; if a safe fix is not feasible, provide analyzer-only guidance.
- The project’s existing rule registration and test harness should be reused.

## Goals / Non-Goals

**Goals:**
- Add 5 new C# 11 analyzers with clear, actionable diagnostics.
- Provide code fixes for rules where a safe mechanical transformation is feasible (raw string literals, required members, UTF-8 string literals; span/list pattern matching if transformation is unambiguous).
- Ensure all rules are language-version gated and covered by unit tests.
- Keep rule IDs, titles, and messages consistent with existing Sharpen conventions.

**Non-Goals:**
- Implement a full refactoring engine for arbitrary string escaping or arbitrary list-pattern rewrites.
- Enforce project-wide style preferences (e.g., always prefer raw strings) beyond the targeted heuristics.
- Introduce runtime dependencies; all changes are analyzer-time only.

## Decisions

1. **Rule structure and registration**
   - Implement each feature as a dedicated analyzer + (optional) code fix provider, following existing `Sharpen.Analyzer/Analyzers/*` and `Sharpen.Analyzer/FixProvider/*` patterns.
   - Register new rules in the central rule catalog so they appear consistently and can be enabled/disabled.

2. **Language version gating**
   - Use the existing language-version detection mechanism (as used by prior C# 9/10 rules) to ensure diagnostics only appear when the compilation language version is C# 11 or later.
   - Add/extend tests similar to existing `CSharpLanguageVersionTests` to validate gating.

3. **Raw string literal suggestion heuristic**
   - Trigger when:
     - The string literal spans multiple lines (contains `\n` escapes or is a verbatim string with newlines), OR
     - The string contains a high density of escape sequences (e.g., `\\`, `\"`, `\n`, `\t`) above a threshold.
   - Code fix:
     - Convert to a raw string literal using `"""` delimiters.
     - Choose delimiter length and indentation conservatively:
       - Start with `"""` and increase delimiter count if the content contains `"""`.
       - Preserve leading indentation by aligning closing delimiter with the start of the literal.
     - If conversion would require complex escaping rules (e.g., interpolated strings, embedded braces), skip fix and keep analyzer-only.

4. **Required members suggestion heuristic**
   - Trigger for auto-properties on reference types where:
     - Property is `public` (or otherwise externally settable),
     - Has a setter (`set;` or `init;`),
     - Is not nullable (or is annotated as non-null),
     - Has no initializer, and
     - Is not already `required`.
   - Prefer targeting DTO-like types:
     - Classes with a public parameterless constructor, or
     - Types with object-initializer usage patterns in the same file (best-effort).
   - Code fix:
     - Add the `required` modifier to the property declaration.
     - Do not attempt to update all call sites (that would be a separate refactoring).

5. **Generic math suggestion**
   - Analyzer-only (initially): detect generic methods performing numeric operators (`+`, `-`, `*`, `/`, comparisons) on type parameters without constraints.
   - Suggest adding `where T : INumber<T>` (or a more specific interface) when:
     - The method is generic over `T`,
     - Operators are used on `T`, and
     - `System.Numerics` is available.
   - Code fix is optional and may be deferred if it requires complex constraint selection or adding `using` directives.

6. **Span/array list-pattern suggestion**
   - Trigger for patterns like:
     - `if (span.Length > 0) { var first = span[0]; ... }`
     - `if (span is { Length: > 0 }) { var first = span[0]; ... }`
   - Prefer analyzer-only unless the rewrite is clearly local and safe.
   - If a fix is implemented, rewrite to `if (span is [var first, .. var rest])` and update subsequent uses when they are trivially mapped.

7. **UTF-8 string literal suggestion**
   - Trigger when code constructs a `byte[]` or `ReadOnlySpan<byte>` from a UTF-8 string in a literal form, e.g.:
     - `new byte[] { 0x68, 0x65, 0x6C, 0x6C, 0x6F }` (ASCII subset)
     - `Encoding.UTF8.GetBytes("...")` with a constant string
   - Code fix:
     - Replace with `ReadOnlySpan<byte> bytes = "..."u8;` when the target type is compatible.
     - If the target is `byte[]`, consider `"..."u8.ToArray()` only if it matches project conventions; otherwise keep analyzer-only.

## Risks / Trade-offs

- **Risk:** Raw string conversion can subtly change semantics (e.g., escape processing, interpolation).
  → **Mitigation:** Only offer fix for non-interpolated, non-verbatim literals where conversion is mechanical; otherwise diagnostic only.

- **Risk:** `required` can introduce compile errors at call sites.
  → **Mitigation:** Keep diagnostic severity as suggestion/info; do not auto-fix across solution.

- **Risk:** Generic math suggestions may be noisy or incorrect for non-numeric “operator-like” types.
  → **Mitigation:** Require evidence of numeric intent (multiple numeric operators, numeric literals, or known numeric APIs) before suggesting.

- **Risk:** List-pattern rewrite may be too invasive.
  → **Mitigation:** Start analyzer-only or restrict fix to very small, local patterns.

- **Trade-off:** Some rules may ship analyzer-only first to ensure correctness.
