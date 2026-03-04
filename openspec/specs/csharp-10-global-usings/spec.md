# csharp-10-global-usings Specification

## Purpose
TBD - created by archiving change csharp-10. Update Purpose after archive.
## Requirements
### Requirement: Detect repeated using directives across multiple files
The analyzer SHALL report a diagnostic when the same `using <namespace>;` directive appears in multiple files within the same project and is a candidate to be replaced by a single `global using <namespace>;`.

#### Scenario: Same using appears in many files
- **WHEN** `using System;` appears in 10 different documents in the same project
- **THEN** the analyzer reports a diagnostic in each document (or a single project-level diagnostic, depending on implementation) indicating it can be made global

#### Scenario: Using appears with aliases and static usings
- **WHEN** `using static System.Math;` and `using IO = System.IO;` appear repeatedly across files
- **THEN** the analyzer reports diagnostics for each repeated directive kind independently

### Requirement: Do not suggest global using when directive is file-specific
The analyzer SHALL NOT report a diagnostic for a `using` directive that is only present in a single file, or that is required to remain local due to conflicting names/aliases in other files.

#### Scenario: Using appears only once
- **WHEN** `using System.Text.Json;` appears in exactly one file
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Conflicting aliases across files
- **WHEN** one file contains `using IO = System.IO;` and another contains `using IO = MyCompany.IO;`
- **THEN** the analyzer does not suggest making `IO` a global alias

### Requirement: Code fix can convert a using directive to global using
When a repeated using directive is flagged, the code fix SHALL replace `using X;` with `global using X;`.

#### Scenario: Convert simple using
- **WHEN** the code fix is applied to `using System;`
- **THEN** the result is `global using System;`

#### Scenario: Convert static using
- **WHEN** the code fix is applied to `using static System.Math;`
- **THEN** the result is `global using static System.Math;`

### Requirement: Code fix does not remove other occurrences automatically
The code fix SHALL only modify the current document unless an explicit “Fix all” is invoked.

#### Scenario: Single-document fix
- **WHEN** the code fix is applied in one file
- **THEN** other files still contain their original `using` directives

