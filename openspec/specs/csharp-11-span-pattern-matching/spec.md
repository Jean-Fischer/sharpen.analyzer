## ADDED Requirements

### Requirement: Suggest list-pattern matching for common span/array indexing patterns
The analyzer SHALL report a diagnostic when code uses common span/array indexing patterns that can be expressed more clearly with list patterns.

#### Scenario: Length check then index
- **WHEN** code checks `span.Length > 0` (or equivalent) and then reads `span[0]`
- **THEN** the analyzer reports a diagnostic suggesting a list pattern such as `span is [var first, ..]`

#### Scenario: Do not suggest when pattern is not applicable
- **WHEN** the target is not an array/span-like type or the indexing pattern is not compatible with list patterns
- **THEN** the analyzer does not report a diagnostic

### Requirement: Optional code fix for unambiguous local rewrite
If the rewrite is unambiguous and local, the code fix provider SHALL offer a fix to rewrite the condition to a list pattern.

#### Scenario: Rewrite simple if-condition
- **WHEN** the diagnostic is reported on a simple `if` condition with a single length check
- **THEN** the code fix rewrites the condition to `is [var first, .. var rest]` (or equivalent) without changing semantics
