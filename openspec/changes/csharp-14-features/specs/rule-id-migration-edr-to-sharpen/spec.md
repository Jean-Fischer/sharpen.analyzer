## ADDED Requirements

### Requirement: Migrate EDR rule IDs to SHARPEN rule IDs
The system SHALL migrate existing rule IDs that use the `EDR` prefix to the `SHARPEN` prefix.

#### Scenario: EDR001 becomes SHARPEN001
- **WHEN** the migration is applied
- **THEN** all references to `EDR001` are replaced with `SHARPEN001`

#### Scenario: Subsequent IDs are assigned deterministically
- **WHEN** multiple `EDR` rules exist
- **THEN** each rule is assigned a unique `SHARPEN` ID by incrementing from the repository’s current highest `SHARPEN` ID (or from `SHARPEN001` if none exist)

### Requirement: Update all references consistently
The migration SHALL update all references to rule IDs across the repository.

#### Scenario: Update code
- **WHEN** diagnostic descriptors, analyzers, fix providers, and safety checkers reference an `EDR` ID
- **THEN** they are updated to the new `SHARPEN` ID

#### Scenario: Update tests
- **WHEN** unit tests assert diagnostic IDs or use baselines containing `EDR` IDs
- **THEN** they are updated to the new `SHARPEN` IDs

#### Scenario: Update documentation
- **WHEN** documentation references `EDR` IDs
- **THEN** it is updated to the new `SHARPEN` IDs and includes a short migration note

### Requirement: No mixed-prefix state
The migration SHALL avoid leaving the repository in a state where both `EDR` and `SHARPEN` IDs are used for the same rule set.

#### Scenario: Single coordinated change
- **WHEN** the migration is complete
- **THEN** no `EDR` rule IDs remain in active rule definitions (except historical references in archived changes)
