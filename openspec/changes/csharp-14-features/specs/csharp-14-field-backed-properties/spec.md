## ADDED Requirements

### Requirement: Detect convertible manual backing-field properties
The analyzer SHALL detect a property that uses a private backing field and can be safely converted to a field-backed property using the C# 14 `field` keyword.

#### Scenario: Simple get/set backing field
- **WHEN** a type contains a private field and a property whose getter returns that field and whose setter assigns to that field
- **THEN** the analyzer reports a diagnostic on the property declaration

#### Scenario: Backing field referenced outside the property
- **WHEN** the backing field is referenced from any code outside the property accessors (including other members or other partial parts)
- **THEN** the analyzer SHALL NOT report a diagnostic

### Requirement: Provide a safe code fix to convert to field-backed property
When the backing field is proven to be used only by the property, the code fix provider SHALL offer a fix that removes the backing field and rewrites the property to use `field` in accessor bodies.

#### Scenario: Apply fix for simple get/set
- **WHEN** the diagnostic is reported for a simple get/set backing-field pattern
- **THEN** the code fix replaces the field + property with a single property using `field` in the accessor bodies

#### Scenario: Preserve trivia and accessibility
- **WHEN** the code fix is applied
- **THEN** the resulting property preserves the original property accessibility, modifiers, attributes, and leading/trailing trivia as closely as possible

### Requirement: Safety checker enforces semantic constraints
A safety checker SHALL validate that the transformation is safe before the code fix is offered.

#### Scenario: Field symbol has no external references
- **WHEN** the safety checker analyzes the backing field symbol
- **THEN** it confirms there are no references outside the property accessors

#### Scenario: Skip when reflection/nameof/attributes reference the field
- **WHEN** the backing field name is used in `nameof`, attributes, or string-literal-based reflection patterns within the containing type
- **THEN** the safety checker SHALL reject the fix

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID and SHALL NOT use the legacy `EDRxxx` prefix.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
