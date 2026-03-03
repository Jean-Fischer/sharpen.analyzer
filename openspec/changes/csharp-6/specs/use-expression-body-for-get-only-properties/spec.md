## ADDED Requirements

### Requirement: Detect get-only properties eligible for expression-bodied form
The analyzer SHALL report a diagnostic when a property is a get-only property and its getter body can be expressed as a single expression.

#### Scenario: Block-bodied getter with a single return statement
- **WHEN** a property has a `get` accessor whose body contains exactly one `return <expr>;` statement
- **THEN** the analyzer reports a diagnostic on the property declaration

#### Scenario: Expression-bodied property is already used
- **WHEN** a property is already declared using expression-bodied syntax (`=>`)
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: Property has a setter
- **WHEN** a property has a `set` accessor (or `init` accessor)
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: Getter contains more than one statement
- **WHEN** a property getter body contains multiple statements (including multiple `return` statements)
- **THEN** the analyzer does NOT report a diagnostic

### Requirement: Provide a code fix to convert eligible get-only properties
The code fix provider SHALL offer a fix that converts an eligible get-only property to an expression-bodied property.

#### Scenario: Apply code fix
- **WHEN** the user applies the code fix on a reported diagnostic
- **THEN** the property is rewritten to `=> <expr>;` preserving trivia (comments/whitespace) as much as possible

#### Scenario: Preserve accessibility and modifiers
- **WHEN** the code fix rewrites the property
- **THEN** the property keeps the same accessibility, modifiers, attributes, and type
