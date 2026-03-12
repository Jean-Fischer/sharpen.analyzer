## ADDED Requirements

### Requirement: Detect lambdas that can omit parameter types when only used for modifiers
The analyzer SHALL detect lambda expressions that specify explicit parameter types solely to allow modifiers (e.g., `ref`, `in`, `out`, `scoped`, `ref readonly`) and where the target delegate type is unambiguous.

#### Scenario: Target delegate type is known
- **WHEN** a lambda is converted to a known delegate type and the parameter types are redundant
- **THEN** the analyzer reports a diagnostic suggesting removal of explicit parameter types while keeping modifiers

#### Scenario: Overload resolution would change
- **WHEN** removing explicit parameter types would change overload resolution or make binding ambiguous
- **THEN** the analyzer SHALL NOT report a diagnostic

### Requirement: Provide a safe code fix to remove redundant parameter types
The code fix provider SHALL offer a fix that removes explicit parameter types from the lambda parameter list while preserving modifiers.

#### Scenario: Apply fix
- **WHEN** the diagnostic is reported for `(ref int x) => Body(x)` in a context with an unambiguous target delegate type
- **THEN** the code fix rewrites it to `(ref x) => Body(x)`

### Requirement: Safety checker validates target typing
A safety checker SHALL validate that the lambda is target-typed and that removing parameter types does not change the inferred delegate type.

#### Scenario: Semantic model confirms same converted type
- **WHEN** the safety checker compares the lambda’s converted type before and after the transformation
- **THEN** it confirms the converted type is unchanged

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
