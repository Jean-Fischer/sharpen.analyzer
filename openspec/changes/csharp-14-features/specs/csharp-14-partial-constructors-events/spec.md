## ADDED Requirements

### Requirement: Informational guidance for partial constructors
The analyzer SHALL detect patterns where a constructor calls a partial method for initialization and suggest considering a C# 14 partial constructor when the project uses source generation patterns.

#### Scenario: Constructor calls partial initialization method
- **WHEN** a constructor body contains a call to a partial method with a name matching common initialization patterns (e.g., `InitializeGenerated`, `OnConstructed`)
- **THEN** the analyzer reports an informational diagnostic suggesting partial constructors as an alternative

### Requirement: Informational guidance for partial events
The analyzer SHALL detect patterns where event add/remove accessors delegate to partial methods and suggest considering a C# 14 partial event.

#### Scenario: Event accessors call partial methods
- **WHEN** an event declaration has explicit add/remove accessors that call partial methods
- **THEN** the analyzer reports an informational diagnostic suggesting partial events as an alternative

### Requirement: No automatic code fix
This capability SHALL NOT provide an automatic code fix.

#### Scenario: Diagnostic has no fix
- **WHEN** the diagnostic is reported
- **THEN** no code fix is offered

### Requirement: Rule ID uses SHARPEN prefix
The rule for this capability SHALL use a `SHARPENxxx` diagnostic ID.

#### Scenario: Rule ID migration
- **WHEN** the rule is introduced or migrated
- **THEN** the diagnostic descriptor uses the `SHARPEN` prefix and the assigned number is consistent with the repository’s rule numbering
