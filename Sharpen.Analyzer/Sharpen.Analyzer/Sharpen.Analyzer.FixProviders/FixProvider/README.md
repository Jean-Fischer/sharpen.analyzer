# Fix provider layering

This folder contains **assembly-layer** code: concrete Roslyn `CodeFixProvider` implementations.

## Layering model

- **Provider layer**: discovery/registration/metadata and safety-pipeline integration.
  - Current location: [`Safety/FixProviderSafety/*`](../Safety/FixProviderSafety/FixProviderSafetyRunner.cs:1)
- **Assembly layer**: concrete Roslyn `CodeFixProvider` implementations.
  - Current location: [`FixProvider/*`](./CSharp10/UseInterpolatedStringCodeFixProvider.cs:1)

## Dependency direction

- Provider-layer code **MUST NOT** reference concrete `CodeFixProvider` implementation types.
- Assembly-layer code **MAY** reference provider-layer abstractions (e.g. safety runner/checkers) to decide whether to offer a fix.
