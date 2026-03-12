## ADDED Requirements

### Requirement: Suggest compound assignment operators for performance-sensitive types
The analyzer SHALL detect types that define binary operators (e.g., `operator +`) and are frequently used with compound assignments (e.g., `x += y`) and suggest considering user-defined compound assignment operators when it could improve performance.

#### Scenario: Type defines operator + and is used with +=
- **WHEN** a type defines `operator +` and code uses `+=` with that type
- **THEN** the analyzer reports an informational diagnostic suggesting a user-defined compound assignment operator

### Requirement: No automatic code fix
This capability SHALL NOT provide an automatic code fix.

#### Scenario: Diagnostic has no fix
- **WHEN** the diagnostic is reported
- **THEN** no code fix is offered

### Requirement: Documentation includes guidance and caveats
Documentation for this rule SHALL include guidance that compound assignment operators must preserve expected semantics and should only be implemented when the type is designed for in-place mutation.

#### Scenario: Docs include trade-offs
- **WHEN** the rule documentation is read
- **THEN** it explains risks of inconsistent operator semantics and recommends manual review

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
