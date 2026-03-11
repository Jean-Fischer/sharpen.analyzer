## MODIFIED Requirements

### Requirement: Safety checks are applied via provider-layer integration
The system SHALL apply fix-provider safety checks through provider-layer integration points, independent of the physical location (assembly/folder) of concrete Roslyn `CodeFixProvider` implementations.

#### Scenario: Safety checks run for providers after provider/assembly split
- **WHEN** fix providers are discovered through the provider layer
- **THEN** the safety-check pipeline MUST still execute the configured safety checks for those providers

#### Scenario: Safety pipeline does not require direct references to concrete fix implementations
- **WHEN** the safety pipeline evaluates a fix provider
- **THEN** it MUST be able to do so without requiring direct references to assembly-layer concrete fix implementations
