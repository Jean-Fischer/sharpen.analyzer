## 1. Rule registration and shared infrastructure

- [x] 1.1 Add rule IDs/metadata entries for the 4 new C# 6 rules in the central rules registry
- [ ] 1.2 Add diagnostic descriptors (title/message/category/severity/help link) for each new rule
- [x] 1.3 Add any shared helper utilities needed for expression-bodied member detection (single-return getter extraction)
- [x] 1.4 Add any shared helper utilities needed for `nameof` detection (string literal extraction, symbol checks)

## 2. Analyzer: use-expression-body-for-get-only-properties

- [x] 2.1 Implement analyzer to detect get-only properties with a single `return <expr>;` getter body
- [x] 2.2 Ensure analyzer excludes properties that already use expression-bodied syntax
- [x] 2.3 Ensure analyzer excludes properties with `set`/`init` accessors
- [x] 2.4 Ensure analyzer excludes getters with multiple statements / non-return statements
- [x] 2.5 Add unit tests: diagnostic reported for eligible get-only property
- [x] 2.6 Add unit tests: no diagnostic for expression-bodied property
- [x] 2.7 Add unit tests: no diagnostic for property with setter/init
- [x] 2.8 Add unit tests: no diagnostic for getter with multiple statements
- [x] 2.9 (If supported) Implement code fix to convert eligible get-only property to expression-bodied property
- [x] 2.10 (If supported) Add unit tests verifying code fix output and trivia preservation

## 3. Analyzer: use-expression-body-for-get-only-indexers

- [x] 3.1 Implement analyzer to detect get-only indexers with a single `return <expr>;` getter body
- [x] 3.2 Ensure analyzer excludes indexers that already use expression-bodied syntax
- [x] 3.3 Ensure analyzer excludes indexers with `set`/`init` accessors
- [x] 3.4 Ensure analyzer excludes getters with multiple statements / non-return statements
- [x] 3.5 Add unit tests: diagnostic reported for eligible get-only indexer
- [x] 3.6 Add unit tests: no diagnostic for expression-bodied indexer
- [x] 3.7 Add unit tests: no diagnostic for indexer with setter/init
- [x] 3.8 Add unit tests: no diagnostic for getter with multiple statements
- [x] 3.9 (If supported) Implement code fix to convert eligible get-only indexer to expression-bodied indexer
- [x] 3.10 (If supported) Add unit tests verifying code fix output and trivia preservation

## 4. Analyzer: use-nameof-expression-for-throwing-argument-exceptions

- [x] 4.1 Implement analyzer to detect `throw new ArgumentXxxException("param")` patterns with string literal parameter names
- [x] 4.2 Add semantic checks to confirm the created type is one of the supported argument exception types
- [x] 4.3 Identify which constructor argument is the parameter-name argument for supported overloads
- [x] 4.4 Add scope checks to ensure the string literal matches an in-scope parameter name
- [x] 4.5 Ensure analyzer excludes cases where `nameof(...)` is already used
- [x] 4.6 Add unit tests: diagnostic reported for `ArgumentNullException("p")` when `p` is a parameter
- [x] 4.7 Add unit tests: diagnostic reported for `ArgumentException("msg", "p")` when `p` is a parameter
- [x] 4.8 Add unit tests: no diagnostic when string literal does not match any parameter
- [x] 4.9 Add unit tests: no diagnostic when `nameof(...)` is already used
- [x] 4.10 (If supported) Implement code fix to replace string literal with `nameof(parameter)`
- [x] 4.11 (If supported) Add unit tests verifying code fix output and trivia preservation

## 5. Analyzer: use-nameof-expression-in-dependency-property-declarations

- [x] 5.1 Implement analyzer to detect `DependencyProperty.Register(...)` calls with string literal property name as first argument
- [x] 5.2 Implement analyzer to detect `DependencyProperty.RegisterAttached(...)` calls with string literal property name as first argument
- [x] 5.3 Add semantic checks to confirm the invoked method is a dependency property registration method
- [x] 5.4 Add symbol checks to confirm a matching CLR property exists on the containing type
- [x] 5.5 Ensure analyzer excludes cases where `nameof(...)` is already used
- [x] 5.6 Add unit tests: diagnostic reported when string literal matches an existing property
- [x] 5.7 Add unit tests: no diagnostic when string literal does not match any property
- [x] 5.8 Add unit tests: no diagnostic when `nameof(...)` is already used
- [x] 5.9 (If supported) Implement code fix to replace string literal with `nameof(Property)`
- [x] 5.10 (If supported) Add unit tests verifying code fix output and trivia preservation

## 6. Samples and validation

- [x] 6.1 Add/extend C# 6 sample code demonstrating each rule in `Sharpen.Analyzer.Sample`
- [ ] 6.2 Verify diagnostics appear in the IDE for each sample
- [ ] 6.3 Verify code fixes appear only for the supported subset (per legacy behavior decision)

## 7. Build and test

- [x] 7.1 Run full test suite and fix any failing tests
- [x] 7.2 Run analyzer package build to ensure no packaging/regression issues
