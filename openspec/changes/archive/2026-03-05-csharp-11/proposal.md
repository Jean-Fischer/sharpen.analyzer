## Why

Sharpen currently doesn’t guide users toward several high-impact C# 11 language features that improve readability, correctness, and performance. Adding targeted analyzers + code fixes helps teams adopt C# 11 idioms safely and consistently.

## What Changes

- Add new analyzers (and code fixes where feasible) for C# 11 (2022) features:
  - Suggest raw string literals for multi-line / heavily-escaped string literals.
  - Suggest `required` members for properties that should be set during initialization.
  - Suggest generic math patterns using `static abstract` members in interfaces (e.g., `INumber<T>`).
  - Suggest span/array pattern matching for common indexing patterns.
  - Suggest UTF-8 string literals (`"..."u8`) instead of byte arrays representing UTF-8 text.
- Add/extend tests to validate diagnostics and code fixes.
- Update rule catalog / language-version gating so rules only trigger when C# 11 is enabled.

## Capabilities

### New Capabilities
- `csharp-11-raw-string-literals`: Detect string literals that are better expressed as raw string literals and offer a fix to convert them.
- `csharp-11-required-members`: Detect properties that should be required at initialization time and offer a fix to add the `required` modifier.
- `csharp-11-generic-math`: Detect numeric-generic patterns that could benefit from generic math interfaces (e.g., `INumber<T>`) and suggest constraints / patterns.
- `csharp-11-span-pattern-matching`: Detect span/array indexing patterns that can be simplified using list patterns and suggest a rewrite.
- `csharp-11-utf8-string-literals`: Detect UTF-8 byte array initializations that can be replaced with `"..."u8` and offer a fix.

### Modified Capabilities

## Impact

- New analyzers and code fix providers in `Sharpen.Analyzer`.
- New/updated unit tests in `Sharpen.Analyzer.Tests`.
- Potential new Roslyn APIs usage for C# 11 syntax nodes (raw string literals, list patterns) and semantic checks.
- Potential dependency on `System.Numerics` generic math interfaces for analysis patterns (no runtime dependency, but analysis must recognize these types).
