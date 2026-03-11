# Fix Provider Safety Check Layer

## Purpose
Define the safety-check layer used by fix providers to decide whether a candidate match is safe to turn into a code fix and/or safe to apply.

This file was created as a minimal baseline so delta specs from experimental changes can be applied and validated.

## Requirements

### Requirement: Safety checks are evaluated before applying a fix
The system SHALL evaluate safety checks before generating or applying a code fix.

#### Scenario: Safety checks run before applying edits
- **WHEN** a code fix is about to be applied
- **THEN** the system SHALL have already evaluated safety checks and only apply edits if checks passed

### Requirement: Safety check failures are observable
When a safety check fails, the system SHALL provide an observable outcome for tests (and optionally internal logging) that indicates the candidate was rejected as unsafe.

#### Scenario: Tests can assert unsafe outcome
- **WHEN** a match candidate is rejected by a safety check
- **THEN** tests can assert an unsafe outcome without requiring any edits to be produced
