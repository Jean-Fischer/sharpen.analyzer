## ADDED Requirements

### Requirement: Suggest raw string literal for multi-line or heavily-escaped strings
The analyzer SHALL report a diagnostic when a string literal can be more readable as a C# 11 raw string literal.

#### Scenario: Multi-line verbatim string
- **WHEN** a verbatim string literal contains one or more newline characters
- **THEN** the analyzer reports a diagnostic suggesting a raw string literal

#### Scenario: Many escape sequences
- **WHEN** a regular string literal contains escape sequences above a readability threshold (e.g., repeated `\\`, `\"`, `\n`, `\t`)
- **THEN** the analyzer reports a diagnostic suggesting a raw string literal

### Requirement: Provide code fix to convert to raw string literal when safe
The code fix provider SHALL offer a fix to convert an eligible string literal to a raw string literal.

#### Scenario: Safe conversion for non-interpolated string
- **WHEN** the diagnostic is reported on a non-interpolated string literal and conversion is mechanical
- **THEN** the code fix replaces the literal with a raw string literal using `"""` delimiters (or longer if needed)

#### Scenario: No fix for unsafe conversion
- **WHEN** the string literal is interpolated or conversion would require non-trivial semantic changes
- **THEN** the code fix is not offered (diagnostic may still be reported)
