## 1. Baseline / Rule Registration

- [x] 1.1 Add C# 11 rule entries to the rule catalog (IDs, titles, messages, categories)
- [x] 1.2 Add language-version gating for C# 11 rules (only run when LangVersion >= 11)
- [x] 1.3 Add/extend language-version unit tests to cover C# 11 gating

## 2. Raw String Literals (csharp-11-raw-string-literals)

- [x] 2.1 Implement analyzer: detect multi-line string literals and report diagnostic
- [x] 2.2 Implement analyzer: detect high escape-density string literals and report diagnostic
- [x] 2.3 Implement code fix: convert eligible string literal to raw string literal (`"""..."""`), choosing delimiter length safely
- [x] 2.4 Add unit tests for diagnostics (multi-line + escape-heavy)
- [x] 2.5 Add unit tests for code fix (simple conversion + delimiter-length edge case)

## 3. Required Members (csharp-11-required-members)

- [x] 3.1 Implement analyzer: detect settable auto-properties without initializer that are likely required
- [x] 3.2 Implement analyzer: exclude properties already marked `required` and other obvious non-candidates
- [x] 3.3 Implement code fix: add `required` modifier with correct modifier ordering
- [x] 3.4 Add unit tests for diagnostics
- [x] 3.5 Add unit tests for code fix

## 4. Generic Math (csharp-11-generic-math)

- [x] 4.1 Implement analyzer: detect numeric operators used on unconstrained type parameters
- [x] 4.2 Implement analyzer: suppress when a compatible generic-math constraint already exists
- [x] 4.3 Implement diagnostic message guidance with example `where T : INumber<T>`
- [x] 4.4 Add unit tests for diagnostics (positive + negative cases)

## 5. Span/List Pattern Matching (csharp-11-span-pattern-matching)

- [x] 5.1 Implement analyzer: detect `Length > 0` (or equivalent) followed by `[0]` access patterns
- [x] 5.2 Implement analyzer: ensure target is array/span-like and list patterns are applicable
- [x] 5.3 (Optional) Implement code fix for the simplest unambiguous `if` condition rewrite to list pattern
- [x] 5.4 Add unit tests for diagnostics
- [x] 5.5 Add unit tests for code fix (if implemented)

## 6. UTF-8 String Literals (csharp-11-utf8-string-literals)

- [x] 6.1 Implement analyzer: detect byte array initializers that decode to valid UTF-8 text (at least ASCII subset)
- [x] 6.2 Implement analyzer: detect `Encoding.UTF8.GetBytes(constantString)` patterns
- [x] 6.3 Implement code fix: replace with `"text"u8` when target type is `ReadOnlySpan<byte>` (or compatible)
- [x] 6.4 Add unit tests for diagnostics
- [x] 6.5 Add unit tests for code fix

## 7. Packaging / Documentation

- [x] 7.1 Update README / rule documentation to include C# 11 rules
- [x] 7.2 Run full test suite and fix any regressions
