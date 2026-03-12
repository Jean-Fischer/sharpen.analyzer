# prefer-params-collections Specification

## Purpose
TBD - created by archiving change csharp-13-features. Update Purpose after archive.
## Requirements
### Requirement: Detect array-based params that can be migrated to collection params
The analyzer SHALL detect method declarations that use `params T[]` and identify cases where migrating to a collection-based `params` (e.g., `ReadOnlySpan<T>`) is likely beneficial.

#### Scenario: Analyzer flags non-public params array
- **WHEN** a `private` or `internal` method declares `params T[] values`
- **THEN** the analyzer reports a suggestion diagnostic indicating a collection-based `params` may be preferable

#### Scenario: Analyzer does not flag public API by default
- **WHEN** a `public` method declares `params T[] values`
- **THEN** the analyzer does not offer an automatic fix and reports either no diagnostic or an info-level diagnostic that is explicitly “no-fix”

### Requirement: Offer a code fix only when the signature change is safe
The fix provider SHALL offer a code fix to migrate `params T[]` to a supported collection-based `params` type only when the change can be proven safe.

#### Scenario: Fix offered for non-public method with safe body usage
- **WHEN** a `private` or `internal` method uses the `params` parameter only via enumeration (e.g., `foreach`) and does not use array-only members
- **THEN** the fix provider offers a code action to change the parameter type to a supported collection-based `params` type and updates in-solution call sites

#### Scenario: Fix not offered when array semantics are used
- **WHEN** the method body uses array-only semantics on the `params` parameter (e.g., indexing, `.Length`, `Array.*`, `GetLength`, `Rank`, `CopyTo`)
- **THEN** the fix provider does not offer the code fix

#### Scenario: Fix not offered when method is public or protected
- **WHEN** the method is `public`, `protected`, or `protected internal`
- **THEN** the fix provider does not offer the code fix

### Requirement: Provide a safety checker for the params collections fix
A safety checker SHALL validate that the transformation is safe before applying the fix.

#### Scenario: Safety checker blocks fix when references cannot be updated
- **WHEN** the method has call sites outside the current solution (or cannot be confidently updated)
- **THEN** the safety checker blocks the fix

#### Scenario: Safety checker blocks fix when target type is unavailable
- **WHEN** the compilation does not reference the target collection type (e.g., `ReadOnlySpan<T>`)
- **THEN** the safety checker blocks the fix

