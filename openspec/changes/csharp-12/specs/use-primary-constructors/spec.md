## ADDED Requirements

### Requirement: Suggest primary constructor for assignment-only constructor
The analyzer SHALL report a diagnostic when a class or struct declares an instance constructor whose body only assigns constructor parameters to instance members, and the type can be expressed using a C# 12 primary constructor without changing semantics.

#### Scenario: Simple constructor assigns parameters to get-only properties
- **WHEN** a type declares a single instance constructor whose body is a sequence of assignments from each parameter to a corresponding get-only auto-property (or backing field used only for that property)
- **THEN** the analyzer reports a diagnostic suggesting a primary constructor

#### Scenario: Constructor contains non-assignment logic
- **WHEN** a constructor body contains any statement other than direct assignments from parameters to instance members (e.g., method calls, conditionals, loops, try/catch)
- **THEN** the analyzer does not report a diagnostic

### Requirement: Code fix converts to primary constructor form
The code fix provider SHALL rewrite eligible types to use a primary constructor and member initializers that reference the primary constructor parameters.

#### Scenario: Convert class with assignment-only constructor
- **WHEN** the user applies the code fix on a diagnostic for an eligible type
- **THEN** the type declaration is rewritten to include a primary constructor parameter list and the assigned members are rewritten to initialize from those parameters

#### Scenario: Preserve accessibility and member names
- **WHEN** the code fix rewrites a type
- **THEN** the resulting type preserves the original type accessibility, member names, and member accessibility
