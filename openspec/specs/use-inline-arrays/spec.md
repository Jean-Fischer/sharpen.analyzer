# use-inline-arrays Specification

## Purpose
TBD - created by archiving change csharp-12. Update Purpose after archive.
## Requirements
### Requirement: Suggest InlineArray attribute for fixed-size buffer structs
The analyzer SHALL report a diagnostic when a struct represents a fixed-size buffer pattern that can be expressed using `[System.Runtime.CompilerServices.InlineArray(N)]` without changing semantics.

#### Scenario: Struct matches canonical fixed-size buffer pattern
- **WHEN** a struct contains only the fields required to represent a fixed-size buffer (e.g., a single `_element0` field or a sequence of `_element0.._element{N-1}` fields) and no additional members that would conflict
- **THEN** the analyzer reports a diagnostic suggesting the InlineArray attribute

#### Scenario: Struct contains additional members
- **WHEN** the struct contains additional fields, methods, or layout attributes that make the transformation ambiguous
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix rewrites struct to InlineArray form
The code fix provider SHALL add the InlineArray attribute and rewrite the struct fields to the InlineArray-compatible representation.

#### Scenario: Apply fix to add InlineArray attribute
- **WHEN** the user applies the code fix on a diagnostic for an eligible struct
- **THEN** the struct is updated to include `[System.Runtime.CompilerServices.InlineArray(N)]`

#### Scenario: Ensure `_element0` field exists
- **WHEN** the code fix rewrites the struct
- **THEN** the struct contains a single backing field named `_element0` of the element type (or equivalent required by InlineArray)

