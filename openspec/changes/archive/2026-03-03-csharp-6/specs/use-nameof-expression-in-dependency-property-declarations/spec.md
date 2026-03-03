## ADDED Requirements

### Requirement: Detect dependency property registrations using string literals for property names
The analyzer SHALL report a diagnostic when a dependency property registration uses a string literal for the property name that can be replaced with `nameof(<property>)`.

#### Scenario: Register call uses string literal matching a property
- **WHEN** code contains a dependency property registration call (e.g., `DependencyProperty.Register`) whose first argument is a string literal `"Foo"`
- **THEN** the analyzer reports a diagnostic if a property named `Foo` exists on the containing type

#### Scenario: RegisterAttached call uses string literal matching a property
- **WHEN** code contains a dependency property registration call (e.g., `DependencyProperty.RegisterAttached`) whose first argument is a string literal `"Foo"`
- **THEN** the analyzer reports a diagnostic if a property named `Foo` exists on the containing type

#### Scenario: String literal does not match a property
- **WHEN** the string literal does not match any property on the containing type
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: nameof is already used
- **WHEN** the property name argument is already `nameof(...)`
- **THEN** the analyzer does NOT report a diagnostic

### Requirement: Provide a code fix to replace string literal with nameof
The code fix provider SHALL offer a fix that replaces the string literal property name argument with `nameof(<property>)`.

#### Scenario: Apply code fix
- **WHEN** the user applies the code fix on a reported diagnostic
- **THEN** the string literal is replaced with `nameof(<property>)` and the code remains semantically equivalent

#### Scenario: Preserve formatting
- **WHEN** the code fix is applied
- **THEN** surrounding trivia (comments/whitespace) is preserved as much as possible
