# partial-properties-indexers-refactoring Specification

## Purpose
TBD - created by archiving change csharp-13-features. Update Purpose after archive.
## Requirements
### Requirement: Detect opportunities for partial properties/indexers refactoring
The analyzer SHALL detect property/indexer patterns that can be expressed using C# 13 partial properties/indexers and offer a refactoring-style action.

#### Scenario: Analyzer identifies split declaration/implementation opportunity
- **WHEN** a partial type contains a property/indexer that is a candidate for splitting into declaring and implementing partial declarations
- **THEN** the analyzer reports an info/suggestion diagnostic indicating a partial member refactoring is available

#### Scenario: Analyzer does not flag when signatures cannot be matched
- **WHEN** the property/indexer signature cannot be matched exactly across partial declarations (name, type, parameters, accessibility, modifiers)
- **THEN** the analyzer does not report this diagnostic

### Requirement: Offer a refactoring-style code action when transformation is safe
The fix provider SHALL offer a code action to transform a supported property/indexer into the C# 13 partial form only when the transformation preserves semantics.

#### Scenario: Fix creates declaring and implementing partial declarations
- **WHEN** the property/indexer is eligible for partial splitting
- **THEN** the fix provider produces a declaring declaration and an implementing declaration with equivalent behavior

#### Scenario: Fix not offered when accessors contain complex logic that cannot be moved safely
- **WHEN** the accessors contain constructs that cannot be safely relocated (e.g., preprocessor-dependent code, file-local dependencies)
- **THEN** the fix provider does not offer the code action

### Requirement: Provide a safety checker for partial member refactorings
A safety checker SHALL validate that the transformation is safe and that the resulting code compiles.

#### Scenario: Safety checker blocks fix when resulting partial member would be invalid
- **WHEN** the transformation would produce an invalid partial member (e.g., missing required accessor, mismatched modifiers)
- **THEN** the safety checker blocks the fix

