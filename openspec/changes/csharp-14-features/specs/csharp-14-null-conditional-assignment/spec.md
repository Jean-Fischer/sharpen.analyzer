## ADDED Requirements

### Requirement: Detect simple guarded assignments convertible to null-conditional assignment
The analyzer SHALL detect the pattern `if (x != null) x.Member = rhs;` (or equivalent with braces) that can be safely replaced with a null-conditional assignment.

#### Scenario: Single assignment statement in if
- **WHEN** an `if` statement checks `x != null` and its body contains exactly one assignment to a member of `x`
- **THEN** the analyzer reports a diagnostic on the `if` statement

#### Scenario: Multiple statements in if body
- **WHEN** the `if` body contains more than one statement
- **THEN** the analyzer SHALL NOT report a diagnostic

### Requirement: Provide a safe code fix to use null-conditional assignment
When the guarded assignment is a single statement and the assignment target is a member access on the checked expression, the code fix provider SHALL offer a fix that rewrites it to `x?.Member = rhs;`.

#### Scenario: Apply fix for property assignment
- **WHEN** the diagnostic is reported for `if (x != null) x.Prop = rhs;`
- **THEN** the code fix replaces it with `x?.Prop = rhs;`

#### Scenario: Preserve evaluation semantics
- **WHEN** the code fix is applied
- **THEN** the rewritten code SHALL preserve the behavior that `rhs` is evaluated only when `x` is non-null

### Requirement: Safety checker validates convertibility
A safety checker SHALL validate that the `if` condition and assignment target match and that the transformation does not change semantics.

#### Scenario: Condition matches the assigned receiver
- **WHEN** the `if` condition checks `x != null`
- **THEN** the safety checker confirms the assignment target receiver is the same `x` expression (after removing parentheses)

#### Scenario: Reject when receiver differs
- **WHEN** the assignment target receiver is not the same expression as the null-checked expression
- **THEN** the safety checker SHALL reject the fix

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
