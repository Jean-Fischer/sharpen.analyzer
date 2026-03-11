# first-safety-check-layer Specification

## Purpose
TBD - created by archiving change first-safety-check-layer. Update Purpose after archive.
## Requirements
### Requirement: First safety check gate runs after match and before fix generation
The system SHALL execute a first safety check gate after a pattern match candidate is produced and before any code fix is generated or applied.

#### Scenario: Match candidate passes first safety check
- **WHEN** a pattern match candidate is produced
- **THEN** the system evaluates the configured first safety checks

#### Scenario: Match candidate fails first safety check
- **WHEN** a pattern match candidate is produced and at least one first safety check fails
- **THEN** the system SHALL NOT generate a code fix for that candidate

### Requirement: Safety checks return structured outcomes
A first safety check SHALL return a structured outcome indicating pass/fail and a reason identifier.

#### Scenario: Safety check returns pass
- **WHEN** a safety check determines the candidate is safe
- **THEN** it returns a pass outcome

#### Scenario: Safety check returns fail with reason
- **WHEN** a safety check determines the candidate is not safe
- **THEN** it returns a fail outcome with a reason identifier

### Requirement: Deterministic and composable evaluation
The system SHALL evaluate first safety checks in a deterministic order and SHALL stop evaluation at the first failing check.

#### Scenario: Multiple checks all pass
- **WHEN** multiple first safety checks are configured and all pass
- **THEN** the candidate proceeds to fix generation

#### Scenario: First failing check stops evaluation
- **WHEN** multiple first safety checks are configured and an early check fails
- **THEN** later checks are not evaluated

