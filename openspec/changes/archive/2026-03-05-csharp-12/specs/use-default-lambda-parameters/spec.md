## ADDED Requirements

### Requirement: Suggest default lambda parameter syntax when applicable
The analyzer SHALL report a diagnostic when a lambda expression can be written using C# 12 default lambda parameter values (e.g., `(int x = 0) => ...`) without changing semantics.

#### Scenario: Explicitly typed lambda parameter has a default value
- **WHEN** a lambda expression has an explicitly typed parameter and a default value can be expressed using C# 12 default parameter syntax
- **THEN** the analyzer reports a diagnostic suggesting default lambda parameter syntax

#### Scenario: Lambda parameter default value is not valid
- **WHEN** the default value is not permitted by the language rules for default parameter values
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix rewrites lambda parameter list to include default values
The code fix provider SHALL rewrite the lambda parameter list to include `= <default>` for eligible parameters.

#### Scenario: Apply fix to add default value syntax
- **WHEN** the user applies the code fix on a diagnostic for an eligible lambda
- **THEN** the lambda parameter list is rewritten to include the default value using C# 12 syntax

#### Scenario: Preserve lambda body
- **WHEN** the code fix rewrites the lambda parameters
- **THEN** the lambda body remains unchanged
