## 1. Baseline / Plumbing

- [x] 1.1 Add C# 10 feature entry (language version + feature registration) in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/CSharpLanguageVersion.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/CSharpLanguageVersion.cs)
- [x] 1.2 Register new rules in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs)
- [x] 1.3 Add/extend sample file(s) for C# 10 in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/)

## 2. File-scoped namespaces (csharp-10-file-scoped-namespaces)

- [x] 2.1 Implement analyzer to detect single top-level block-scoped namespace eligible for conversion
- [x] 2.2 Implement code fix to convert `namespace X { ... }` to `namespace X;` preserving trivia
- [x] 2.3 Add tests: single namespace with multiple members (class + struct + interface)
- [x] 2.4 Add tests: nested namespaces inside outer namespace (outer becomes file-scoped, inner remains block-scoped)
- [x] 2.5 Add tests: two sibling namespaces in one file (no diagnostic)

## 3. Global using directives (csharp-10-global-usings)

- [x] 3.1 Implement multi-document analyzer to collect `using` directives per project and detect repeats (threshold >= 2)
- [x] 3.2 Define normalization key for using directives (namespace + static + alias) and ensure stable grouping
- [x] 3.3 Implement code fix to convert `using` to `global using` (simple, static, alias)
- [x] 3.4 Add tests: repeated `using System;` across 3 documents triggers diagnostics
- [x] 3.5 Add tests: repeated `using static System.Math;` across 2 documents triggers diagnostics
- [x] 3.6 Add tests: conflicting aliases across files does NOT suggest global alias
- [x] 3.7 Add tests: applying fix in one file does not remove other occurrences (unless fix-all)

## 4. Record structs (csharp-10-record-structs)

- [x] 4.1 Implement analyzer heuristics for value-object structs (fields/get-only props, minimal behavior)
- [x] 4.2 Implement code fix to convert `struct` keyword to `record struct` preserving modifiers/attributes
- [x] 4.3 Add tests: simple public field-based struct triggers diagnostic and fix
- [x] 4.4 Add tests: struct with get-only auto-properties + constructor triggers diagnostic
- [x] 4.5 Add tests: mutable struct with setters does NOT trigger diagnostic

## 5. Extended property patterns (csharp-10-extended-property-patterns)

- [x] 5.1 Implement analyzer for `x is T t && t.Prop OP value` pattern
- [x] 5.2 Implement code fix rewriting to `x is { Prop: OP value }`
- [x] 5.3 Implement analyzer for null-conditional nested equality pattern (e.g., `x?.A?.B == c`) when safe
- [x] 5.4 Implement code fix rewriting to nested property pattern `x is { A: { B: c } }`
- [x] 5.5 Add tests: simple age check rewrite
- [x] 5.6 Add tests: nested access with null-conditionals rewrite (include a null case to ensure semantics)
- [x] 5.7 Add tests: side-effecting access (e.g., `GetPerson().Age > 18`) does NOT trigger

## 6. Constant interpolated strings (csharp-10-constant-interpolated-strings)

- [x] 6.1 Implement analyzer for `string.Format` → interpolated string rewrite
- [x] 6.2 Implement analyzer for concatenation chain → interpolated string rewrite
- [x] 6.3 Implement const-specific analyzer path: only offer const rewrite when all holes are compile-time constants
- [x] 6.4 Implement code fix to rewrite to interpolated string preserving format specifiers (e.g., `{n:000}`)
- [x] 6.5 Add tests: `string.Format("Hello, {0}!", name)` → `$"Hello, {name}!"`
- [x] 6.6 Add tests: concatenation chain → interpolated string
- [x] 6.7 Add tests: const concatenation of literals offers const-safe rewrite
- [x] 6.8 Add tests: const with variable does NOT trigger const diagnostic

## 7. Integration / Quality

- [x] 7.1 Ensure all diagnostics have consistent IDs, titles, messages, and help links (if used)
- [x] 7.2 Add “complex scenario” sample snippets for each feature in the sample project
- [x] 7.3 Run test suite and fix any failures
- [x] 7.4 Add/adjust documentation entries in [`Readme.md`](Readme.md) if the project lists supported C# versions/features