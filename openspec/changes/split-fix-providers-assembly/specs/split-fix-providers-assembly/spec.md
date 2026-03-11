## ADDED Requirements

### Requirement: Provider vs assembly layering is explicit
The system SHALL define and enforce a clear separation between fix-provider provider-layer code and fix-provider assembly-layer code.

#### Scenario: Provider layer does not depend on concrete Roslyn code fix implementations
- **WHEN** a type is part of the provider layer
- **THEN** it MUST NOT reference concrete Roslyn `CodeFixProvider` implementation types

#### Scenario: Assembly layer contains concrete Roslyn code fix implementations
- **WHEN** a type is part of the assembly layer
- **THEN** it MUST be allowed to reference Roslyn APIs and implement `CodeFixProvider`

### Requirement: Provider layer exposes stable metadata for fix providers
The provider layer SHALL expose metadata sufficient to discover, group, and integrate fix providers into the safety pipeline without requiring direct references to concrete fix implementations.

#### Scenario: Discovery uses metadata only
- **WHEN** the system enumerates available fix providers
- **THEN** it MUST be able to do so using provider-layer metadata without instantiating or referencing concrete fix implementations

### Requirement: Refactor preserves fix behavior
The refactor SHALL preserve the observable behavior of existing fixes unless explicitly documented as a breaking change.

#### Scenario: Existing diagnostics still have corresponding fixes
- **WHEN** a diagnostic previously had a fix provider
- **THEN** the same diagnostic MUST still have a fix provider after the refactor

#### Scenario: Fix output remains equivalent
- **WHEN** a fix is applied before and after the refactor
- **THEN** the produced code changes MUST be functionally equivalent

## MODIFIED Requirements

### Requirement: Safety pipeline integration remains functional
The system SHALL continue to run the fix-provider safety pipeline with the refactored provider/assembly split.

#### Scenario: Safety checks run for discovered fix providers
- **WHEN** fix providers are discovered through the provider layer
- **THEN** the safety-check pipeline MUST still execute the configured safety checks for those providers

#### Scenario: Safety pipeline does not require assembly-layer references
- **WHEN** the safety pipeline evaluates a fix provider
- **THEN** it MUST be able to do so without requiring direct references to assembly-layer concrete fix implementations
