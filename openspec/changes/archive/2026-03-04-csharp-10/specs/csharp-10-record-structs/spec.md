## ADDED Requirements

### Requirement: Detect struct candidates for record struct
The analyzer SHALL report a diagnostic when a `struct` declaration represents a value object and can be expressed as a `record struct` without changing its public surface area beyond the syntactic transformation.

#### Scenario: Simple field-based value object
- **WHEN** a file contains `public struct Point { public int X; public int Y; }`
- **THEN** the analyzer reports a diagnostic on `struct Point`

#### Scenario: Struct with read-only properties and constructor
- **WHEN** a file contains `public struct Money { public decimal Amount {get;} public string Currency {get;} public Money(decimal a,string c){Amount=a;Currency=c;} }`
- **THEN** the analyzer reports a diagnostic suggesting `record struct`

### Requirement: Do not suggest record struct for mutable or behavior-heavy structs
The analyzer SHALL NOT report a diagnostic when the struct is clearly mutable (e.g., has settable properties) or contains significant behavior that would not map cleanly to a primary-constructor record struct.

#### Scenario: Mutable struct with setters
- **WHEN** a file contains `public struct S { public int X {get; set;} }`
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Struct with many methods and invariants
- **WHEN** a file contains a struct with multiple methods and private state used to enforce invariants
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix converts struct to record struct
When the diagnostic is reported, the code fix SHALL convert `struct` to `record struct`.

#### Scenario: Convert keyword only
- **WHEN** the code fix is applied to `public struct Point { public int X; public int Y; }`
- **THEN** the result starts with `public record struct Point` and preserves the body

#### Scenario: Preserve attributes and modifiers
- **WHEN** the code fix is applied to `[Serializable] internal readonly struct S { }`
- **THEN** the result is `[Serializable] internal readonly record struct S { }`