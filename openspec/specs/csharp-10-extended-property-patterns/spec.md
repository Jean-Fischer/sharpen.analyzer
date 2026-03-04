# csharp-10-extended-property-patterns Specification

## Purpose
TBD - created by archiving change csharp-10. Update Purpose after archive.
## Requirements
### Requirement: Detect nested member access patterns that can be expressed as property patterns
The analyzer SHALL report a diagnostic when code uses nested member access in a type test or conditional that can be equivalently expressed using a property pattern.

#### Scenario: is-pattern with relational pattern
- **WHEN** code checks `person is Person p && p.Age > 18`
- **THEN** the analyzer reports a diagnostic suggesting `person is { Age: > 18 }`

#### Scenario: Nested property access
- **WHEN** code checks `order.Customer.Address.Country == "FR"` after a null-check chain
- **THEN** the analyzer reports a diagnostic suggesting a nested property pattern like `order is { Customer: { Address: { Country: "FR" } } }`

### Requirement: Do not suggest property patterns when semantics would change
The analyzer SHALL NOT report a diagnostic when rewriting to a property pattern would change evaluation order, introduce repeated evaluation, or rely on side effects.

#### Scenario: Member access has side effects
- **WHEN** code checks `GetPerson().Age > 18`
- **THEN** the analyzer does not report a diagnostic

#### Scenario: Uses local variable for performance
- **WHEN** code assigns `var c = order.Customer;` and then checks multiple properties on `c`
- **THEN** the analyzer does not report a diagnostic if a property pattern would reduce clarity or change null-handling intent

### Requirement: Code fix rewrites to property pattern
When the diagnostic is reported, the code fix SHALL rewrite the expression to an equivalent property pattern.

#### Scenario: Rewrite simple age check
- **WHEN** the code fix is applied to `if (person is Person p && p.Age > 18) { }`
- **THEN** the result is `if (person is { Age: > 18 }) { }`

#### Scenario: Rewrite nested access with null checks
- **WHEN** the code fix is applied to `if (order?.Customer?.Address?.Country == "FR") { }`
- **THEN** the result uses a property pattern that preserves null-safety (e.g., `order is { Customer: { Address: { Country: "FR" } } }`)

