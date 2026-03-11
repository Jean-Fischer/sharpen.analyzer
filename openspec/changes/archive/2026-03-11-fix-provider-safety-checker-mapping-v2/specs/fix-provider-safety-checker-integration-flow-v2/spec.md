## ADDED Requirements

### Requirement: Safety checker runs before diagnostic reporting
The system SHALL run the mapped safety checker before the analyzer reports a diagnostic.

#### Scenario: Unsafe context suppresses diagnostic
- **WHEN** an analyzer detects a potential issue that would normally produce a diagnostic
- **AND** the mapped safety checker evaluates the context as unsafe
- **THEN** the analyzer SHALL NOT report the diagnostic

#### Scenario: Safe context allows diagnostic
- **WHEN** an analyzer detects a potential issue that would normally produce a diagnostic
- **AND** the mapped safety checker evaluates the context as safe
- **THEN** the analyzer SHALL report the diagnostic

### Requirement: Safety checker runs before code action offering
The system SHALL run the mapped safety checker before a fix provider offers a code action.

#### Scenario: Unsafe context suppresses code action
- **WHEN** a fix provider is asked to register code actions for a diagnostic
- **AND** the mapped safety checker evaluates the context as unsafe
- **THEN** the fix provider SHALL NOT register any code actions

#### Scenario: Safe context allows code action
- **WHEN** a fix provider is asked to register code actions for a diagnostic
- **AND** the mapped safety checker evaluates the context as safe
- **THEN** the fix provider SHALL register the code action(s)

### Requirement: Integration flow example is documented
The system SHALL document an integration flow example using NullCheckAnalyzer, NullCheckSafetyChecker, and NullCheckFixProvider.

#### Scenario: Example shows end-to-end gating
- **WHEN** the example flow is followed
- **THEN** it SHALL show the safety checker being invoked before diagnostic reporting and before code action offering

### Requirement: Safety evaluation outcome is observable
The system SHALL provide an observable outcome of safety evaluation suitable for tests (e.g., safe/unsafe + reason).

#### Scenario: Tests can assert safety outcome
- **WHEN** a test runs the safety checker for a given code sample
- **THEN** it SHALL be able to assert whether the outcome is safe or unsafe and why
