# csharp-8-analyzers-migration Specification

## Purpose
TBD - created by archiving change csharp8. Update Purpose after archive.
## Requirements
### Requirement: Inventory legacy C# 8 analyzers
The system SHALL provide a complete inventory of all analyzers in the legacy Sharpen project that target C# 8, including their diagnostic IDs and whether a code fix exists.

#### Scenario: Inventory is complete and actionable
- **WHEN** the migration work starts
- **THEN** a list exists that includes every legacy C# 8 analyzer type, its diagnostic ID(s), and a link/path to its legacy source

### Requirement: Migrate analyzer implementations
The system SHALL include migrated analyzer implementations in the new project, preserving diagnostic behavior (trigger conditions, reported locations, IDs, titles, messages, categories, and default severities).

#### Scenario: Analyzer behavior parity
- **WHEN** a migrated analyzer is executed on code that triggers the legacy analyzer
- **THEN** the new analyzer reports the same diagnostic ID and equivalent message and location as the legacy analyzer

### Requirement: Migrate code fixes when available
For each legacy C# 8 analyzer that has an associated code fix, the system SHALL include an equivalent code fix provider in the new project.

#### Scenario: Code fix produces equivalent output
- **WHEN** a migrated diagnostic is raised and the user applies the code fix
- **THEN** the resulting code matches the legacy code fix output (or is semantically equivalent if formatting differs)

### Requirement: Migrate and validate tests
The system SHALL include unit tests for each migrated analyzer (and code fix where applicable) in the new test project, ensuring regression protection.

#### Scenario: Tests cover migrated analyzers
- **WHEN** the test suite is executed
- **THEN** there is at least one passing test that validates each migrated analyzer’s diagnostic behavior

### Requirement: Build and packaging integration
The system SHALL register migrated analyzers and code fixes so they are included in the build outputs and distributed packages produced by this repository.

#### Scenario: Analyzers are included in outputs
- **WHEN** the solution is built and packaged
- **THEN** the migrated analyzers are present and loadable by the consumer (e.g., via NuGet/VSIX as applicable)

### Requirement: Compatibility with current Roslyn dependencies
The migrated analyzers and code fixes SHALL compile and run against the Roslyn package versions used by this repository.

#### Scenario: Compilation succeeds without legacy Roslyn pinning
- **WHEN** the solution is built using the repository’s current dependencies
- **THEN** the migrated analyzer and code fix projects compile successfully without requiring a downgrade to legacy Roslyn versions

