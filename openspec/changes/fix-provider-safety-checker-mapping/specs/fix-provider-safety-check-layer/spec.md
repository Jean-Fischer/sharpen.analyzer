## MODIFIED Requirements

### Requirement: Safety checks are mandatory for code fixes
The system MUST ensure that every code fix provider has an associated safety checker and that the safety checker is executed before the fix is applied.

#### Scenario: Fix provider without safety checker is rejected
- **WHEN** a fix provider is registered/discovered and no safety checker is mapped to it
- **THEN** the system reports a validation failure and the fix provider is not eligible for fix application

#### Scenario: Safety checker is executed before applying fix
- **WHEN** a fix is requested for a fix provider
- **THEN** the system resolves the safety checker via the fix-provider-to-safety-checker mapping and executes it before applying the fix

#### Scenario: Safety check failure prevents fix application
- **WHEN** the mapped safety checker returns failure for a requested fix
- **THEN** the system does not apply the fix

### Requirement: Safety checker resolution is mapping-based
The system SHALL resolve safety checkers for fix providers exclusively via the one-to-one mapping registry.

#### Scenario: No ad-hoc safety checker resolution
- **WHEN** the system needs to determine which safety checker to run for a fix provider
- **THEN** it uses the mapping registry and does not use naming conventions or manual branching
