## ADDED Requirements

### Requirement: One-to-one mapping between fix providers and safety checkers
The system SHALL define an explicit one-to-one mapping between each fix provider and exactly one safety checker.

#### Scenario: Fix provider has exactly one mapped safety checker
- **WHEN** the system loads the fix provider → safety checker mapping
- **THEN** each fix provider type is associated with exactly one safety checker type

#### Scenario: Mapping rejects duplicates
- **WHEN** a fix provider is registered with more than one safety checker
- **THEN** the system SHALL fail validation and surface an actionable error

### Requirement: Mapping completeness is validated
The system SHALL validate that all supported fix providers are present in the mapping.

#### Scenario: Missing mapping entry
- **WHEN** a fix provider exists without a mapping entry
- **THEN** the system SHALL fail validation and surface which fix provider is missing

### Requirement: Canonical mapping table is maintained
The change SHALL maintain a canonical mapping table that documents the intended fix provider ↔ safety checker pairs.

#### Scenario: Mapping table is updated when adding a new fix provider
- **WHEN** a new fix provider is introduced
- **THEN** the mapping table SHALL be updated to include the new fix provider and its safety checker

### Requirement: Safety checker is the single source of truth for gating
The mapped safety checker SHALL be the single source of truth used to gate both diagnostic reporting and code action offering.

#### Scenario: Analyzer and fix provider consult the same checker
- **WHEN** the analyzer pipeline evaluates whether to report a diagnostic
- **THEN** it SHALL consult the mapped safety checker
- **WHEN** the fix provider evaluates whether to offer a code action
- **THEN** it SHALL consult the same mapped safety checker

### Requirement: Mapping includes the initial set of fix providers
The mapping table SHALL include at least the following fix provider families: NullCheck, CollectionExpression, StringInterpolation, SwitchExpression, Linq.

#### Scenario: Initial mapping table contains required entries
- **WHEN** the mapping table is reviewed
- **THEN** it SHALL contain entries for NullCheck, CollectionExpression, StringInterpolation, SwitchExpression, and Linq
