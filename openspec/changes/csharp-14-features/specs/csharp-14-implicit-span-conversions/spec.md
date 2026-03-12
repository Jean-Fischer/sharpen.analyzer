## ADDED Requirements

### Requirement: Detect redundant explicit span conversions
The analyzer SHALL detect redundant explicit conversions to `Span<T>` / `ReadOnlySpan<T>` where an implicit conversion is available and the explicit conversion adds no semantic value.

#### Scenario: Redundant AsSpan call
- **WHEN** code passes `array.AsSpan()` to a parameter of type `ReadOnlySpan<T>` (or `Span<T>`) and the array can be implicitly converted
- **THEN** the analyzer reports a diagnostic suggesting removal of the explicit conversion

#### Scenario: Conversion required
- **WHEN** the explicit conversion is required for overload resolution or to select a different API
- **THEN** the analyzer SHALL NOT report a diagnostic

### Requirement: Provide a safe code fix to remove redundant conversions
The code fix provider SHALL offer a fix that removes the redundant conversion while preserving formatting and trivia.

#### Scenario: Apply fix
- **WHEN** the diagnostic is reported for `M(array.AsSpan())` where `M` expects `ReadOnlySpan<T>`
- **THEN** the code fix rewrites it to `M(array)`

### Requirement: Safety checker validates no span escape or binding change
A safety checker SHALL validate that removing the explicit conversion does not change binding and does not introduce span lifetime/escape issues.

#### Scenario: Same invoked symbol
- **WHEN** the safety checker compares the invoked method symbol before and after the transformation
- **THEN** it confirms the same method overload is selected

#### Scenario: Reject when overload changes
- **WHEN** removing the conversion changes overload resolution
- **THEN** the safety checker SHALL reject the fix

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
