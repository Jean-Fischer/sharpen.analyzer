## MODIFIED Requirements

### Requirement: Safety checks are evaluated before applying a fix
The system SHALL evaluate safety checks before generating or applying a code fix, including an explicit first-pass gate that runs immediately after a match candidate is produced.

#### Scenario: First-pass safety gate blocks fix generation
- **WHEN** a match candidate is produced and the first-pass safety gate fails
- **THEN** the system SHALL NOT generate a code fix for that candidate

#### Scenario: Safety checks run before applying edits
- **WHEN** a code fix is about to be applied
- **THEN** the system SHALL have already evaluated safety checks and only apply edits if checks passed

### Requirement: Safety check failures are observable
When a safety check fails, the system SHALL provide an observable outcome for tests (and optionally internal logging) that indicates the candidate was rejected as unsafe.

#### Scenario: Tests can assert unsafe outcome
- **WHEN** a match candidate is rejected by a safety check
- **THEN** tests can assert an unsafe outcome without requiring any edits to be produced

#### Scenario: Optional logging hook
- **WHEN** a match candidate is rejected by a safety check
- **THEN** the system MAY emit an internal log entry including the failure reason identifier
