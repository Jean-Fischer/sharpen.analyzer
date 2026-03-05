# csharp-11-generic-math

## Purpose

Define expected analyzer behavior for suggesting C# 11 generic math constraints.

## Requirements

### Requirement: Suggest generic math constraints for numeric operations on type parameters
The analyzer SHALL report a diagnostic when a generic type parameter is used with numeric operators and could benefit from generic math interfaces.

#### Scenario: Operator usage on unconstrained type parameter
- **WHEN** a generic method uses numeric operators (e.g., `+`, `-`, `*`, `/`) on a type parameter `T` without an appropriate constraint
- **THEN** the analyzer reports a diagnostic suggesting adding a constraint such as `where T : INumber<T>`

#### Scenario: Do not suggest when already constrained
- **WHEN** the type parameter already has a generic math constraint compatible with the operators used
- **THEN** the analyzer does not report a diagnostic

### Requirement: Provide guidance in diagnostic message
The analyzer diagnostic message SHALL include an example constraint (e.g., `INumber<T>`) and indicate that `static abstract` interface members enable the operator usage.

#### Scenario: Message includes example
- **WHEN** the diagnostic is reported
- **THEN** the message includes a suggested interface (e.g., `INumber<T>`) and a brief rationale
