# use-nameof-expression-for-throwing-argument-exceptions Specification

## Purpose
TBD - created by archiving change csharp-6. Update Purpose after archive.
## Requirements
### Requirement: Detect argument exceptions using string literals for parameter names
The analyzer SHALL report a diagnostic when an argument exception is thrown and the parameter name argument is a string literal that can be replaced with `nameof(<parameter>)`.

#### Scenario: Throw new ArgumentNullException with string literal
- **WHEN** code contains `throw new ArgumentNullException("p")` and `p` is a parameter in the current method/constructor/lambda
- **THEN** the analyzer reports a diagnostic on the string literal argument

#### Scenario: Throw new ArgumentException with string literal parameter name
- **WHEN** code contains `throw new ArgumentException("message", "p")` and `p` is a parameter in the current method/constructor/lambda
- **THEN** the analyzer reports a diagnostic on the string literal parameter name argument

#### Scenario: String literal does not match an in-scope parameter
- **WHEN** the string literal does not match any parameter name in scope
- **THEN** the analyzer does NOT report a diagnostic

#### Scenario: nameof is already used
- **WHEN** the parameter name argument is already `nameof(...)`
- **THEN** the analyzer does NOT report a diagnostic

### Requirement: Provide a code fix to replace string literal with nameof
The code fix provider SHALL offer a fix that replaces the string literal parameter name with `nameof(<parameter>)`.

#### Scenario: Apply code fix
- **WHEN** the user applies the code fix on a reported diagnostic
- **THEN** the string literal is replaced with `nameof(<parameter>)` and the code remains semantically equivalent

#### Scenario: Preserve formatting
- **WHEN** the code fix is applied
- **THEN** surrounding trivia (comments/whitespace) is preserved as much as possible

