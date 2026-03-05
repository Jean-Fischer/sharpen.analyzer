## Why

C# 12 introduces several language features that simplify common patterns, but Sharpen.Analyzer currently does not guide users toward these modern idioms. Adding analyzers and fixes for these features helps users adopt C# 12 quickly and consistently.

## What Changes

- Add analyzer + code fix to suggest **primary constructors** when a type’s constructor only assigns parameters to properties/fields.
- Add analyzer + code fix to suggest **collection expressions** (`[...]`) when array/collection initializers can be expressed using C# 12 collection expressions.
- Add analyzer + code fix to suggest **default lambda parameters** using C# 12 syntax (`(int x = 0) => ...`) when applicable.
- Add analyzer + code fix to suggest **inline arrays** via `[System.Runtime.CompilerServices.InlineArray(N)]` for fixed-size buffer-like structs.
- Update rule registration / fix provider wiring as needed so the new rules are discoverable and fixes are offered.

## Capabilities

### New Capabilities
- `use-primary-constructors`: Detect constructors that only assign parameters to members and suggest converting to a primary constructor.
- `use-collection-expressions`: Detect eligible array/collection initializers and suggest C# 12 collection expressions.
- `use-default-lambda-parameters`: Detect lambdas that can use default parameter values and suggest C# 12 default lambda parameter syntax.
- `use-inline-arrays`: Detect fixed-size buffer structs and suggest using the `InlineArray` attribute pattern.

### Modified Capabilities

<!-- None. -->

## Impact

- New analyzers and code fix providers in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/:1) and [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/:1).
- Rule registration updates in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/:1).
- New/updated unit tests in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/:1).
- Requires Roslyn support for parsing/binding C# 12 syntax; may require updating language version/test compilation settings if not already configured.
