## Context

Sharpen is a Roslyn-based analyzer/code-fix suite focused on modernizing C# code and teaching newer language features through safe, mechanical refactorings.

This change targets a set of C# 9 language features that can be suggested as modernizations:

- Init-only setters
- Record types
- Top-level statements
- Pattern matching enhancements (relational/logical/not patterns)
- Target-typed `new`

The proposal document ([`openspec/changes/csharp-9/proposal.md`](openspec/changes/csharp-9/proposal.md:1)) defines the intended rule families, examples, and a test matrix.

Constraints and realities to design around:

- Roslyn analyzers must be conservative: avoid semantic changes and avoid suggestions that require cross-solution reasoning unless explicitly implemented.
- Some transformations are inherently “project-level” (e.g., top-level statements) and can be risky if applied too broadly.
- The codebase already has patterns for analyzers and code fixes (diagnostic IDs, `DiagnosticDescriptor`, `CodeFixProvider`, and unit tests).
- All rules must be language-version gated (C# 9+ only).

## Goals / Non-Goals

**Goals:**

- Provide a consistent implementation approach for the five C# 9 rule families:
  - analyzer detection strategy
  - code fix strategy
  - language-version gating strategy
  - test strategy and sample coverage
- Keep each rule’s first iteration conservative and safe, with clear extension points for later improvements.
- Ensure each rule family has:
  - at least one “happy path” sample
  - multiple negative samples
  - edge-case tests (trivia, directives, generics, nullable contexts)

**Non-Goals:**

- Implementing the analyzers/code fixes in this artifact.
- Aggressive whole-solution dataflow or inheritance analysis (e.g., “no derived types in solution”) unless already supported by existing infrastructure.
- Large-scale project migration tooling (e.g., converting entire projects to top-level statements across multiple files).

## Decisions

### 1) One rule family → one diagnostic ID (initially)

**Decision:** Start with one diagnostic per rule family (5 diagnostics total), each with a single primary code fix.

**Rationale:**

- Keeps the surface area small and consistent with prior “C# version” changes.
- Allows incremental expansion later (e.g., splitting record detection into multiple diagnostics) without blocking initial delivery.

**Alternatives considered:**

- Multiple diagnostics per family from day one (more precise but higher complexity and more tests).

### 2) Conservative detection first, then expand

**Decision:** Implement conservative detection criteria for each rule, matching the “Notes: start conservative” guidance in the proposal.

**Rationale:**

- Avoids false positives that would erode trust.
- Many of these transformations require non-trivial semantic checks to be safe.

**Alternatives considered:**

- Implement full semantic/dataflow analysis immediately (higher correctness potential but significantly more engineering and test burden).

### 3) Language version gating via compilation options

**Decision:** Gate diagnostics and fixes on C# language version >= 9.

**Rationale:**

- Prevents offering fixes that would not compile.
- Keeps behavior predictable across multi-targeted solutions.

**Implementation approach (high-level):**

- In analyzers: check `context.Compilation` parse options (C#) and language version.
- In code fixes: also gate in `RegisterCodeFixesAsync` to avoid offering fixes if analyzer gating is bypassed.

**Alternatives considered:**

- Only gate in analyzer (simpler, but code fix could still be invoked in some edge scenarios).

### 4) Init-only setters: start with `private set;` auto-properties

**Decision:** The first iteration flags only auto-properties with `private set;` (and no custom setter body) and offers `init;`.

**Rationale:**

- This is a mechanical, local transformation.
- Avoids needing whole-solution assignment tracking.

**Alternatives considered:**

- Dataflow-based “only assigned in constructors/object initializers” (valuable but more complex; can be a later enhancement).

### 5) Record types: start with `sealed class` data containers

**Decision:** The first iteration flags only `sealed class` types that are “pure data containers” (properties + constructor) and have no base class.

**Rationale:**

- Converting a class hierarchy to records is risky.
- Records change equality semantics; limiting to sealed data containers reduces risk.

**Alternatives considered:**

- Support non-sealed classes with no derived types (requires solution-wide symbol search).
- Support classes with base classes (requires careful mapping; often not safe).

### 6) Top-level statements: only for single-file, global-namespace `Program` patterns

**Decision:** Only offer the top-level statements fix when:

- the file contains only a `Program` class with a single `Main` method
- the file is in the global namespace (no `namespace` declaration)
- there are no other types/members
- there is no `typeof(Program)` or other references that require `Program` to remain a type

**Rationale:**

- This is the most “project-like” transformation in the set.
- Being conservative avoids breaking code that depends on `Program` as a symbol.

**Alternatives considered:**

- Support namespace-wrapped `Program` by keeping namespace and moving statements (not possible in C# 9 without changing structure; C# 10 file-scoped namespaces change the landscape).
- Support multi-file programs (out of scope).

### 7) Pattern matching enhancements: focus on local, side-effect-free rewrites

**Decision:** Implement a small set of safe rewrites:

- `x != null` → `x is not null`
- `!(x is T)` → `x is not T`
- range checks on the same side-effect-free expression:
  - `x >= a && x <= b` → `x is >= a and <= b`
  - `x < a || x > b` → `x is < a or > b`

**Rationale:**

- These are common, readable improvements.
- Side-effect-free constraint avoids changing evaluation count.

**Alternatives considered:**

- More advanced boolean-to-pattern rewrites (e.g., mixing `is` type checks with null checks) which can be ambiguous and require careful precedence handling.

### 8) Target-typed `new`: start with unambiguous target contexts

**Decision:** Offer `new()` only when the target type is explicit and unambiguous:

- explicit local/field/property type with initializer
- assignment to a symbol with a known type
- return statement where return type is known

Avoid (initially):

- `var x = new T()`
- argument position unless overload resolution is provably unchanged
- cases relying on implicit conversions (policy decision; default to conservative)

**Rationale:**

- Target-typed `new` is usually safe, but overload resolution and conversions can introduce subtle changes.

**Alternatives considered:**

- Support argument position broadly (requires deeper semantic checks and more tests).

### 9) Test strategy: “samples as tests”

**Decision:** For each diagnostic, maintain:

- a minimal “sample” snippet used in documentation and mirrored in tests
- a broad test matrix (positive/negative/edge)

**Rationale:**

- Ensures examples remain correct and compile.
- Prevents drift between docs and behavior.

## Risks / Trade-offs

- **[Record conversion changes equality semantics]** → Mitigation: restrict to sealed data containers; add negative tests for custom equality.
- **[Top-level statements can be project-sensitive]** → Mitigation: restrict to single-file global-namespace patterns; add many negative tests.
- **[Pattern rewrites can change evaluation count]** → Mitigation: only rewrite side-effect-free expressions; add negative tests for invocations.
- **[Target-typed `new` can affect overload resolution]** → Mitigation: avoid argument-position rewrites initially; add negative tests for overload sets.
- **[Language version gating mistakes]** → Mitigation: add tests that ensure no diagnostics/fixes under C# 8 language version.
