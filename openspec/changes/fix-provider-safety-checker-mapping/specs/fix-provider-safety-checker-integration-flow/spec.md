## ADDED Requirements

### Requirement: Safety check is executed before applying a fix
The system MUST execute the mapped safety checker for a fix provider before applying that fix.

#### Scenario: Safety check passes and fix is applied
- **WHEN** a fix is requested for a fix provider and the mapped safety checker returns success
- **THEN** the system applies the fix

#### Scenario: Safety check fails and fix is not applied
- **WHEN** a fix is requested for a fix provider and the mapped safety checker returns failure
- **THEN** the system does not apply the fix

### Requirement: Integration flow resolves safety checker via mapping
The system SHALL resolve the safety checker for a fix provider exclusively via the mapping registry.

#### Scenario: No ad-hoc resolution
- **WHEN** the integration flow needs a safety checker for a fix provider
- **THEN** it resolves it via the mapping registry and not via naming conventions or manual branching

### Requirement: Integration flow surfaces mapping/validation failures
The system MUST surface a clear failure when the mapping is incomplete or ambiguous.

#### Scenario: Missing mapping prevents fix application
- **WHEN** a fix is requested for a fix provider that has no mapped safety checker
- **THEN** the system reports a validation failure and does not apply the fix

#### Scenario: Ambiguous mapping prevents fix application
- **WHEN** the mapping is invalid (e.g., duplicates)
- **THEN** the system reports a validation failure and does not apply any affected fixes

### Requirement: Integration flow is consistent across entry points
The system SHALL ensure that all code paths that apply fixes use the same mapping-based safety-check flow.

#### Scenario: All fix application entry points use the same flow
- **WHEN** a fix is applied through any supported entry point
- **THEN** the mapped safety checker is executed before applying the fix
