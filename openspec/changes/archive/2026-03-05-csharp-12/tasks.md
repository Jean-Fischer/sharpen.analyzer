## 1. Baseline / Infrastructure

- [x] 1.1 Verify solution builds with Roslyn/C# 12 support (update package references and/or LangVersion in test harness if needed)
- [x] 1.2 Add new rule IDs + descriptors for the four C# 12 rules in the central rule registry (and ensure they appear in any exported rule list)
- [x] 1.3 Ensure code fix provider wiring supports the new diagnostics (export attributes, FixableDiagnosticIds, registration patterns)
- [x] 1.4 Add shared helper utilities (if needed) for C# 12 syntax nodes (primary constructor, collection expression, inline array attribute)

## 2. Use Primary Constructors

- [x] 2.1 Implement analyzer to detect assignment-only instance constructors eligible for primary constructor conversion
- [x] 2.2 Implement code fix to rewrite type declaration to primary constructor form and rewrite members to initialize from parameters
- [x] 2.3 Add unit tests: positive cases (simple class, multiple properties) validating diagnostic + fix output
- [x] 2.4 Add unit tests: negative cases (extra constructor logic, multiple constructors, base/this chaining with non-trivial args)

## 3. Use Collection Expressions

- [x] 3.1 Implement analyzer to detect eligible array initializers and (if feasible) target-typed collection initializers for collection expressions
- [x] 3.2 Implement code fix to rewrite initializer to `[...]` while preserving element expressions and trivia
- [x] 3.3 Add unit tests: arrays (new T[] { ... }, new[] { ... }) → `[...]`
- [x] 3.4 Add unit tests: negative cases where rewrite is unsafe or not supported

## 4. Use Default Lambda Parameters

- [x] 4.1 Implement analyzer to detect lambdas eligible for default parameter syntax (explicitly typed parameters with valid default values)
- [x] 4.2 Implement code fix to rewrite lambda parameter list to include `= <default>` while preserving the lambda body
- [x] 4.3 Add unit tests: positive cases for default parameter syntax
- [x] 4.4 Add unit tests: negative cases (invalid defaults, cases requiring adding types)

## 5. Use Inline Arrays

- [x] 5.1 Implement analyzer to detect canonical fixed-size buffer structs eligible for InlineArray
- [x] 5.2 Implement code fix to add `[System.Runtime.CompilerServices.InlineArray(N)]` and normalize fields to `_element0` representation
- [x] 5.3 Add unit tests: positive cases for InlineArray rewrite
- [x] 5.4 Add unit tests: negative cases (additional members, conflicting layout/attributes)

## 6. Validation / Polish

- [x] 6.1 Run full test suite and fix any failures
- [x] 6.2 Ensure diagnostics have clear titles/messages and consistent severity/category
- [x] 6.3 Add/adjust documentation or README references if the project lists supported rules/features
