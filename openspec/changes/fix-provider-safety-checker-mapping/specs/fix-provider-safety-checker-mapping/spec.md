## ADDED Requirements

### Requirement: Mapping registry exists and is authoritative
The system SHALL provide a single, authoritative mapping between each fix provider and exactly one safety checker.

#### Scenario: Mapping is defined centrally
- **WHEN** the system is initialized
- **THEN** it loads the mapping from a single registry (not from ad-hoc wiring)

### Requirement: Mapping is one-to-one
The system MUST enforce a one-to-one relationship between fix providers and safety checkers.

#### Scenario: Fix provider without safety checker is rejected
- **WHEN** a fix provider is discovered/registered and no safety checker is mapped to it
- **THEN** the system reports a validation failure identifying the fix provider

#### Scenario: Safety checker mapped to multiple fix providers is rejected
- **WHEN** the mapping contains the same safety checker mapped to more than one fix provider
- **THEN** the system reports a validation failure identifying the safety checker and the conflicting fix providers

#### Scenario: Duplicate fix provider mapping is rejected
- **WHEN** the mapping contains more than one entry for the same fix provider
- **THEN** the system reports a validation failure identifying the fix provider and the conflicting entries

### Requirement: Mapping is deterministic
The mapping resolution MUST be deterministic and MUST NOT depend on discovery order.

#### Scenario: Resolution is stable across runs
- **WHEN** fix providers are discovered in different orders
- **THEN** resolving the safety checker for a given fix provider yields the same safety checker

### Requirement: Mapping is discoverable by the integration flow
The system SHALL expose the mapping to the fix-application pipeline so it can resolve the correct safety checker for a fix provider.

#### Scenario: Integration flow can resolve checker
- **WHEN** the fix-application pipeline needs the safety checker for a fix provider
- **THEN** it resolves it via the mapping registry
