## MODIFIED Requirements

### Requirement: Safety checks gate fix providers
The system SHALL gate fix providers behind safety checks so that code actions are only offered when the target context is safe.

#### Scenario: Fix provider is gated by its mapped safety checker
- **WHEN** a fix provider is invoked to register code actions
- **THEN** the system SHALL run the mapped safety checker first
- **AND** the fix provider SHALL only register code actions if the safety checker outcome is safe

#### Scenario: Fix provider is blocked when safety checker is unsafe
- **WHEN** the mapped safety checker outcome is unsafe
- **THEN** the fix provider SHALL not register any code actions

### Requirement: Safety checks gate diagnostic reporting
The system SHALL gate diagnostic reporting behind the same safety checks used for fix providers.

#### Scenario: Analyzer is gated by the mapped safety checker
- **WHEN** an analyzer is about to report a diagnostic that has an associated fix provider
- **THEN** the system SHALL run the mapped safety checker first
- **AND** the diagnostic SHALL only be reported if the safety checker outcome is safe

#### Scenario: Diagnostic is suppressed when safety checker is unsafe
- **WHEN** the mapped safety checker outcome is unsafe
- **THEN** the diagnostic SHALL not be reported

### Requirement: Safety check mapping is explicit and validated
The system SHALL maintain an explicit mapping between fix providers and safety checkers and validate it for completeness and uniqueness.

#### Scenario: Mapping validation fails on missing entry
- **WHEN** a fix provider exists without a mapping entry
- **THEN** the system SHALL fail validation and surface which fix provider is missing

#### Scenario: Mapping validation fails on duplicate entry
- **WHEN** a fix provider is mapped to multiple safety checkers
- **THEN** the system SHALL fail validation and surface an actionable error
