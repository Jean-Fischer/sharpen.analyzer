## ADDED Requirements

### Requirement: Suggest required modifier for properties that must be initialized
The analyzer SHALL report a diagnostic when a property is likely intended to be set during initialization and is not marked `required`.

#### Scenario: Public auto-property without initializer
- **WHEN** a class declares a settable auto-property with no initializer and it is not marked `required`
- **THEN** the analyzer reports a diagnostic suggesting adding `required`

#### Scenario: Do not suggest when already required
- **WHEN** a property is already marked `required`
- **THEN** the analyzer does not report a diagnostic

### Requirement: Provide code fix to add required modifier
The code fix provider SHALL offer a fix to add the `required` modifier to the property declaration.

#### Scenario: Add required to property
- **WHEN** the diagnostic is reported on a property declaration
- **THEN** the code fix inserts the `required` modifier in the correct modifier order
