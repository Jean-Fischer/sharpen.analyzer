## ADDED Requirements

### Requirement: Suggest organizing extension methods into extension blocks
The analyzer SHALL detect static classes that contain multiple extension methods for the same receiver type and suggest organizing them into a C# 14 extension block.

#### Scenario: Multiple extension methods for same receiver
- **WHEN** a static class contains two or more extension methods targeting the same receiver type
- **THEN** the analyzer reports an informational diagnostic suggesting an extension block

#### Scenario: Mixed receiver types
- **WHEN** a static class contains extension methods for many unrelated receiver types
- **THEN** the analyzer MAY report a diagnostic only when a clear dominant receiver type exists

### Requirement: Optional code fix for mechanical refactor
If the transformation is purely mechanical and does not change semantics, the code fix provider MAY offer a fix to move eligible extension methods into an extension block.

#### Scenario: Apply fix for simple methods
- **WHEN** all selected extension methods are in the same file and have no preprocessor constraints that would be broken by moving
- **THEN** the code fix rewrites them into an extension block while preserving method bodies and accessibility

### Requirement: Safety checker limits scope
A safety checker SHALL restrict the code fix to cases where the refactor is local and does not require cross-file symbol movement.

#### Scenario: Reject cross-file refactor
- **WHEN** extension methods are spread across multiple partial declarations or files
- **THEN** the safety checker SHALL reject the fix (diagnostic may still be reported)

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
