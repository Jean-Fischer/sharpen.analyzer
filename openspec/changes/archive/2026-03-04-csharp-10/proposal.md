## Why

Sharpen currently targets up to C# 9; adding C# 10 analyzers/code fixes keeps the tool relevant and helps users modernize codebases with the new language constructs.
This change focuses on practical, real-world refactorings and includes richer tests/samples to validate behavior beyond “happy path” snippets.

## What Changes

- Add new analyzers + code fixes for selected C# 10 features:
  - File-scoped namespaces: detect block-scoped namespaces and offer file-scoped conversion.
  - Global using directives: detect repeated `using` directives across multiple files and suggest consolidating into `global using`.
  - Record structs: detect structs that are effectively value objects and suggest `record struct`.
  - Extended property patterns: detect nested property access patterns in conditionals/switches and suggest property patterns.
  - Constant interpolated strings: detect `string.Format` / concatenation patterns that can be expressed as constant interpolated strings.
- Add tests and sample code for complex scenarios (multiple namespaces, multiple files, mixed usings, nested patterns, etc.) to validate analyzers and fixes in realistic code.

## Capabilities

### New Capabilities
- `csharp-10-file-scoped-namespaces`: Detect block-scoped namespaces and offer a code fix to convert to file-scoped namespaces.
- `csharp-10-global-usings`: Detect repeated `using` directives across files and suggest moving them to a single `global using` location.
- `csharp-10-record-structs`: Detect candidate structs that should be `record struct` and offer a code fix.
- `csharp-10-extended-property-patterns`: Detect nested member access patterns in conditionals/switches and suggest property patterns.
- `csharp-10-constant-interpolated-strings`: Detect format/concatenation patterns that can be expressed as constant interpolated strings and offer a code fix.

### Modified Capabilities

<!-- none -->

## Impact

- New analyzers and code fix providers in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/) and [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/).
- New/updated tests in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/).
- Potential updates to rule registration in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs).
- Sample updates in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/).