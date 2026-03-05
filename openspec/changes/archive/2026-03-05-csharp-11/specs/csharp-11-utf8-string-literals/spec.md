## ADDED Requirements

### Requirement: Suggest UTF-8 string literal for UTF-8 byte data
The analyzer SHALL report a diagnostic when code represents UTF-8 text as a byte array or equivalent and can be replaced with a UTF-8 string literal.

#### Scenario: Byte array literal representing ASCII/UTF-8 text
- **WHEN** a `byte[]` initializer contains a sequence of bytes that decodes to valid UTF-8 text
- **THEN** the analyzer reports a diagnostic suggesting `"..."u8`

#### Scenario: Encoding.UTF8.GetBytes on constant string
- **WHEN** code calls `Encoding.UTF8.GetBytes` with a compile-time constant string
- **THEN** the analyzer reports a diagnostic suggesting `"..."u8`

### Requirement: Provide code fix to replace with u8 literal when type-compatible
The code fix provider SHALL offer a fix to replace the expression with a UTF-8 string literal when the target type is compatible.

#### Scenario: Replace with ReadOnlySpan<byte>
- **WHEN** the target type is `ReadOnlySpan<byte>` (or compatible)
- **THEN** the code fix replaces the initializer with `"text"u8`

#### Scenario: No fix when target type is incompatible
- **WHEN** the target type requires a `byte[]` and no safe conversion is available
- **THEN** the code fix is not offered (diagnostic may still be reported)
