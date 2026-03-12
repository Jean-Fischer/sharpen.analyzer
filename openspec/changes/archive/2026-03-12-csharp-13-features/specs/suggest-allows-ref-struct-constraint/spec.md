## ADDED Requirements

### Requirement: Suggest allows ref struct constraint opportunities
The analyzer SHALL detect generic methods/types where adding the `allows ref struct` anti-constraint could enable span-like usage and provide guidance.

#### Scenario: Analyzer flags generic API that could accept ref struct type arguments
- **WHEN** a generic API is used in a way that would benefit from accepting `ref struct` type arguments (e.g., span-like types)
- **THEN** the analyzer reports an info/suggestion diagnostic describing the opportunity and constraints

#### Scenario: Analyzer message includes ref-safety guidance
- **WHEN** the diagnostic is reported
- **THEN** the message includes guidance that `allows ref struct` is a library design decision and must be reviewed for ref-safety implications

### Requirement: Optional code action is review-required
If a code action is provided, it SHALL be labeled as requiring review.

#### Scenario: Code action inserts allows ref struct constraint
- **WHEN** the user invokes the code action
- **THEN** the fix inserts `where T : allows ref struct` (or equivalent) and labels the action as “requires review”
