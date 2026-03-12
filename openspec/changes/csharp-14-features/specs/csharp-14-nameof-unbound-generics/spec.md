## ADDED Requirements

### Requirement: Detect nameof on closed generic used only for type name
The analyzer SHALL detect `nameof(SomeGeneric<TArg>)` where the closed generic argument is used only to satisfy the compiler and the intent is to obtain the generic type name.

#### Scenario: nameof on closed generic
- **WHEN** code contains `nameof(List<int>)` (or any closed constructed generic type) in a `nameof` expression
- **THEN** the analyzer reports a diagnostic suggesting `nameof(List<>)`

#### Scenario: Non-generic nameof
- **WHEN** code contains `nameof(SomeType)` where `SomeType` is not a constructed generic type
- **THEN** the analyzer SHALL NOT report a diagnostic

### Requirement: Provide a code fix to use unbound generic type in nameof
The code fix provider SHALL offer a fix that replaces the constructed generic type with the unbound generic type form.

#### Scenario: Apply fix
- **WHEN** the diagnostic is reported for `nameof(Dictionary<string, int>)`
- **THEN** the code fix replaces it with `nameof(Dictionary<,>)`

### Requirement: Safety checker ensures transformation is purely syntactic
A safety checker SHALL validate that the `nameof` expression is a compile-time constant and that replacing the type syntax does not change binding.

#### Scenario: nameof remains constant
- **WHEN** the fix is applied
- **THEN** the resulting `nameof` expression still binds and produces the same string value

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
