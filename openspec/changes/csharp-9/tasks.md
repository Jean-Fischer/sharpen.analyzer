## 1. Baseline / Infrastructure

- [x] 1.1 Identify existing analyzer + code fix patterns to mirror (diagnostic IDs, descriptors, registration, test harness)
- [x] 1.2 Add C# 9 language-version gating helper (shared utility) and unit tests for gating behavior
- [x] 1.3 Define diagnostic IDs, titles, messages, and categories for the 5 C# 9 rule families

## 2. Rule: UseInitOnlySetter

- [x] 2.1 Implement analyzer: detect auto-properties with `get; private set;` eligible for `init;`
- [x] 2.2 Implement code fix: replace `set;` accessor with `init;` preserving trivia/attributes/modifiers
- [x] 2.3 Add positive tests: diagnostic + fix for minimal sample and variations (attributes, structs, nullable context)
- [x] 2.4 Add negative tests: non-trivial setter, assigned outside constructor, interface implementation
- [x] 2.5 Add documentation sample snippet used by tests (ensure sample compiles)

## 3. Rule: UseRecordType

- [x] 3.1 Implement analyzer: detect eligible `sealed class` data containers (properties + ctor, no base class)
- [x] 3.2 Implement code fix: convert to `record` (initially allow either block record or primary-ctor record per design)
- [x] 3.3 Add positive tests: minimal sample and variations (attributes, XML docs, init-only properties)
- [x] 3.4 Add negative tests: behavioral methods, mutable fields, base class, custom equality patterns
- [x] 3.5 Add documentation sample snippet used by tests (ensure sample compiles)

## 4. Rule: UseTopLevelStatements

- [x] 4.1 Implement analyzer: detect single-file global-namespace `Program` with single `Main` and eligible body
- [x] 4.2 Implement code fix: remove `Program`/`Main` boilerplate and lift statements to top-level preserving usings/trivia
- [x] 4.3 Add positive tests: simple `Main`, `args` usage, `return`/exit code (if supported)
- [x] 4.4 Add negative tests: namespace present, additional types/members, `typeof(Program)` usage, directives
- [x] 4.5 Add documentation sample snippet used by tests (ensure sample compiles)

## 5. Rule: UseCSharp9PatternMatching

- [x] 5.1 Implement analyzer: detect `expr != null` and `!(expr is T)` patterns for side-effect-free `expr`
- [x] 5.2 Implement analyzer: detect range checks (`x >= a && x <= b`, `x < a || x > b`) for side-effect-free `x`
- [x] 5.3 Implement code fix: rewrite to `is not null`, `is not T`, and relational/logical patterns with correct parentheses
- [x] 5.4 Add positive tests: null checks, negated is-pattern, range checks (constants/variables)
- [x] 5.5 Add negative tests: side-effect expressions, non-identical repeated expressions, precedence edge cases
- [x] 5.6 Add documentation sample snippet used by tests (ensure sample compiles)

## 6. Rule: UseTargetTypedNew

- [x] 6.1 Implement analyzer: detect `new T(...)` in explicit-type local/field/property initializers
- [x] 6.2 Implement analyzer: detect eligible assignments/returns where target type is unambiguous
- [x] 6.3 Implement code fix: replace `new T(...)` with `new(...)` / `new()` and preserve initializers/trivia
- [x] 6.4 Add positive tests: locals, fields, properties, returns, object/collection initializers, nested generics
- [x] 6.5 Add negative tests: `var` declarations, arrays, overload ambiguity in argument position
- [x] 6.6 Add documentation sample snippet used by tests (ensure sample compiles)

## 7. Integration / Quality

- [x] 7.1 Ensure all diagnostics are suppressed under C# 8 language version (tests)
- [x] 7.2 Run full test suite and fix any flaky/formatting issues
- [x] 7.3 Add/verify README or docs references for the new C# 9 rules (if project has a rule catalog)
