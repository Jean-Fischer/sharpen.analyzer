## ADDED Requirements

### Requirement: Detect format/concatenation patterns that can be expressed as interpolated strings
The analyzer SHALL report a diagnostic when code uses `string.Format(...)` or string concatenation that can be replaced by an interpolated string expression.

#### Scenario: string.Format with simple placeholders
- **WHEN** code contains `var s = string.Format("Hello, {0}!", name);`
- **THEN** the analyzer reports a diagnostic suggesting `$"Hello, {name}!"`

#### Scenario: Concatenation chain
- **WHEN** code contains `var s = "Hello, " + name + "!";`
- **THEN** the analyzer reports a diagnostic suggesting `$"Hello, {name}!"`

### Requirement: Detect constant interpolated string candidates
The analyzer SHALL report a diagnostic when a `const string` is assigned using concatenation or `string.Format` with only constant inputs, and can be replaced by a constant interpolated string.

#### Scenario: const concatenation
- **WHEN** code contains `const string s = "Hello, " + "World" + "!";`
- **THEN** the analyzer reports a diagnostic suggesting `const string s = $"Hello, {"World"}!"` or a simplified constant string if applicable

#### Scenario: const with constant placeholders
- **WHEN** code contains `const string s = string.Format("{0}-{1}", "A", "B");`
- **THEN** the analyzer reports a diagnostic suggesting `const string s = $"{"A"}-{"B"}"`

### Requirement: Do not suggest constant interpolated strings when not constant
The analyzer SHALL NOT report a constant-interpolation diagnostic when any interpolated hole would be non-constant.

#### Scenario: const uses variable
- **WHEN** code contains `const string s = "Hello, " + name;`
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix rewrites to interpolated string
When the diagnostic is reported, the code fix SHALL rewrite the expression to an interpolated string while preserving semantics.

#### Scenario: Preserve format specifiers
- **WHEN** the code fix is applied to `string.Format("{0:000}", n)`
- **THEN** the result is `$"{n:000}"`