## Why

Sharpen.Analyzer currently focuses on C# 5 async/await guidance, but it does not yet provide recommendations for key C# 6 language features that improve readability and correctness.
Adding C# 6 rules expands the analyzer’s coverage and helps users modernize codebases incrementally.

## What Changes

- Add 4 new C# 6 diagnostics (and code fixes only where the legacy Sharpen project had them):
  - Expression-bodied members: suggest expression bodies for get-only properties.
  - Expression-bodied members: suggest expression bodies for get-only indexers.
  - `nameof`: suggest `nameof(...)` when throwing argument exceptions.
  - `nameof`: suggest `nameof(...)` in dependency property declarations.
- Add unit tests and sample project examples for each rule.

## Capabilities

### New Capabilities
- `use-expression-body-for-get-only-properties`: Detect get-only properties that can be simplified to an expression-bodied member.
- `use-expression-body-for-get-only-indexers`: Detect get-only indexers that can be simplified to an expression-bodied member.
- `use-nameof-expression-for-throwing-argument-exceptions`: Detect argument exceptions that pass a parameter name as a string literal and can be replaced with `nameof`.
- `use-nameof-expression-in-dependency-property-declarations`: Detect dependency property declarations that use string literals for property names and can be replaced with `nameof`.

### Modified Capabilities

<!-- None -->

## Impact

- New analyzers and (potentially) new code fix providers in the `Sharpen.Analyzer` projects.
- New rule IDs and entries in the central rules registry.
- Additional unit tests in `Sharpen.Analyzer.Tests`.
- Additional sample code in `Sharpen.Analyzer.Sample` to validate behavior in the IDE.
