## ADDED Requirements

### Requirement: C# 7 feature set is migrated from upstream Sharpen
Sharpen.Analyzer MUST include the C# 7 feature analyzers and code fixes that exist in the upstream/original Sharpen repository, migrated into the new project structure.

#### Scenario: Upstream C# 7 rules are discoverable in the new project
- **WHEN** a developer searches the new codebase for the migrated C# 7 analyzers and code fixes
- **THEN** the rules are present under the standard folders for analyzers and fix providers

### Requirement: Migrated analyzers are registered and produce diagnostics
Each migrated C# 7 analyzer MUST be registered in the analyzer package and MUST report diagnostics consistent with the upstream behavior.

#### Scenario: Analyzer reports a diagnostic for a matching code pattern
- **WHEN** a code sample contains a pattern targeted by a migrated C# 7 analyzer
- **THEN** the analyzer reports the expected diagnostic at the expected location

### Requirement: Migrated code fixes apply the expected transformation
For each migrated analyzer that has an upstream code fix, Sharpen.Analyzer MUST provide a corresponding code fix that produces the same transformation as upstream.

#### Scenario: Code fix updates code to the recommended C# 7 form
- **WHEN** a diagnostic produced by a migrated C# 7 analyzer is fixed via the code fix provider
- **THEN** the resulting code matches the expected transformation and compiles

### Requirement: Unit tests exist for each migrated rule
Each migrated C# 7 analyzer/code fix pair MUST have unit tests in the test project that validate both diagnostic reporting and code fix output (when applicable).

#### Scenario: Test suite validates diagnostics and fixes
- **WHEN** the unit test suite is executed
- **THEN** tests covering each migrated C# 7 rule pass and assert the expected diagnostics and fixes

### Requirement: C# 7 samples are available in the sample project
Sharpen.Analyzer.Sample MUST include C# 7 sample snippets under a dedicated `csharp7` folder, mirroring the organization used for C# 6 samples.

#### Scenario: Sample project contains C# 7 folder and examples
- **WHEN** a developer opens the sample project
- **THEN** a `csharp7` folder exists and contains sample code demonstrating the migrated C# 7 rules
