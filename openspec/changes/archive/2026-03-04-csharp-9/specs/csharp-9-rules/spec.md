## ADDED Requirements

### Requirement: C# 9 rules are language-version gated
All diagnostics and code fixes introduced by this change SHALL only be offered when the compilation language version is C# 9 or higher.

#### Scenario: Project language version is C# 8
- **WHEN** a project is compiled with language version C# 8 (or lower)
- **THEN** no diagnostics from this change are reported

#### Scenario: Project language version is C# 9
- **WHEN** a project is compiled with language version C# 9 (or higher)
- **THEN** diagnostics from this change MAY be reported when their rule-specific conditions are met

### Requirement: Use init-only setters for eligible auto-properties
The analyzer SHALL report a diagnostic when an auto-property uses a `private set;` accessor and can be safely converted to an `init;` accessor.

#### Scenario: Auto-property with private setter
- **WHEN** a type declares an auto-property with `get; private set;`
- **THEN** the analyzer reports a diagnostic on the property

#### Scenario: Code fix converts private set to init
- **WHEN** the code fix is applied to a diagnostic reported for `get; private set;`
- **THEN** the setter accessor is replaced with `init;` and the code remains compilable

#### Scenario: Property is assigned after construction
- **WHEN** a property is assigned in a method (not a constructor)
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Property has non-trivial setter logic
- **WHEN** a property setter contains a body with statements or expressions beyond a trivial auto-property
- **THEN** the analyzer does not report a diagnostic

### Requirement: Use record types for eligible sealed data classes
The analyzer SHALL report a diagnostic when a `sealed class` is a pure data container and can be safely converted to a `record`.

#### Scenario: Sealed data class with get-only properties and constructor
- **WHEN** a `sealed class` contains only get-only auto-properties and a constructor that assigns them
- **THEN** the analyzer reports a diagnostic on the type declaration

#### Scenario: Code fix converts sealed class to record
- **WHEN** the code fix is applied to a diagnostic reported for an eligible sealed data class
- **THEN** the type declaration is converted to a `record` form and the code remains compilable

#### Scenario: Class contains behavioral methods
- **WHEN** a class contains non-trivial methods (beyond simple property/constructor patterns)
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Class has a non-record base class
- **WHEN** a class inherits from a base class
- **THEN** the analyzer does not report a diagnostic

### Requirement: Use top-level statements for eligible entry points
The analyzer SHALL report a diagnostic when a file contains a simple `Program` class with a single `Main` method that can be safely converted to top-level statements.

#### Scenario: Global-namespace Program with single Main
- **WHEN** a file contains only `class Program` with a single `static Main` method and no namespace declaration
- **THEN** the analyzer reports a diagnostic on the `Program` class or `Main` method

#### Scenario: Code fix removes Program/Main boilerplate
- **WHEN** the code fix is applied to a diagnostic reported for an eligible entry point
- **THEN** the `Program` class and `Main` method are removed and the `Main` body statements become top-level statements

#### Scenario: File contains a namespace declaration
- **WHEN** the entry point is declared inside a `namespace` block
- **THEN** the analyzer does not report a diagnostic

#### Scenario: File contains additional types or members
- **WHEN** the file contains additional types or the `Program` class contains additional members
- **THEN** the analyzer does not report a diagnostic

### Requirement: Use C# 9 pattern matching enhancements for eligible boolean expressions
The analyzer SHALL report a diagnostic when a boolean expression can be safely rewritten using C# 9 pattern matching enhancements without changing semantics.

#### Scenario: Null check rewritten to not pattern
- **WHEN** code contains `expr != null` where `expr` is side-effect free
- **THEN** the analyzer reports a diagnostic and the code fix rewrites it to `expr is not null`

#### Scenario: Negated is-pattern rewritten to is not
- **WHEN** code contains `!(expr is T)` where `expr` is side-effect free
- **THEN** the analyzer reports a diagnostic and the code fix rewrites it to `expr is not T`

#### Scenario: Range check rewritten to relational and-pattern
- **WHEN** code contains `x >= a && x <= b` where `x` is side-effect free and syntactically identical in both comparisons
- **THEN** the analyzer reports a diagnostic and the code fix rewrites it to `x is >= a and <= b`

#### Scenario: Expression has side effects
- **WHEN** code contains `Get() != null` or other expressions with potential side effects
- **THEN** the analyzer does not report a diagnostic

### Requirement: Use target-typed new for eligible object creation
The analyzer SHALL report a diagnostic when an object creation expression redundantly specifies a type that is already known from the target context.

#### Scenario: Explicitly typed local variable initializer
- **WHEN** code contains `T x = new T(...)` or `T x = new T { ... }`
- **THEN** the analyzer reports a diagnostic and the code fix rewrites it to `T x = new(...)` or `T x = new() { ... }`

#### Scenario: var declaration
- **WHEN** code contains `var x = new T(...)`
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Argument position with overload ambiguity
- **WHEN** an object creation expression is used as an argument in a call with multiple overloads where target typing could affect overload resolution
- **THEN** the analyzer does not report a diagnostic
