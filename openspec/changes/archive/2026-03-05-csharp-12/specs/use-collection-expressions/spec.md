## ADDED Requirements

### Requirement: Suggest collection expressions for eligible initializers
The analyzer SHALL report a diagnostic when an array or collection initializer can be rewritten as a C# 12 collection expression (`[...]`) without changing semantics.

#### Scenario: Array initializer can be expressed as collection expression
- **WHEN** code initializes an array using an initializer expression that is representable as a collection expression
- **THEN** the analyzer reports a diagnostic suggesting a collection expression

#### Scenario: Initializer is not safely representable
- **WHEN** the initializer contains constructs that would change meaning when rewritten (e.g., requires non-trivial builder semantics or unsupported element forms)
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix rewrites initializer to collection expression
The code fix provider SHALL rewrite eligible initializers to use `[...]` while preserving the element expressions and target type.

#### Scenario: Replace array creation initializer with `[...]`
- **WHEN** the user applies the code fix on a diagnostic for an eligible array initializer
- **THEN** the initializer is rewritten to a collection expression with the same elements

#### Scenario: Preserve element order and trivia
- **WHEN** the code fix rewrites an initializer
- **THEN** the resulting collection expression preserves element order and retains leading/trailing trivia where possible
