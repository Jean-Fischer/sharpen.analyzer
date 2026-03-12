## 1. Rule ID migration (EDR → SHARPEN)

- [x] 1.1 Inventory existing rule IDs: locate all `EDR\d+` and `SHARPEN\d+` usages and determine the current highest `SHARPEN` ID
- [x] 1.2 Define the mapping table for `EDRxxx` → `SHARPENxxx` (ensure `EDR001` → `SHARPEN001`, then increment deterministically)
- [x] 1.3 Update diagnostic descriptors and rule catalogs to use the new `SHARPEN` IDs
- [x] 1.4 Update fix providers and safety checkers to reference the new `SHARPEN` IDs
- [x] 1.5 Update unit tests and baselines to assert the new `SHARPEN` IDs
- [x] 1.6 Update documentation to replace `EDR` references and add a short migration note
- [x] 1.7 Add/refresh any internal mapping documentation (if the project maintains a rule index)

## 2. C# 14: Field-backed properties (`field` keyword)

- [x] 2.1 Add analyzer rule (new `SHARPENxxx`) to detect eligible backing-field + property patterns
- [x] 2.2 Implement safety checker: verify backing field is private and referenced only within the property accessors (including partial type parts)
- [x] 2.3 Implement code fix: remove backing field and rewrite property accessors to use `field`
- [x] 2.4 Add tests: diagnostic triggers on simple patterns
- [x] 2.5 Add tests: no diagnostic when backing field is referenced elsewhere / in `nameof` / in attributes
- [x] 2.6 Add tests: code fix output preserves modifiers/attributes/trivia
- [x] 2.7 Update docs: rule description, safe-to-fix vs do-not-fix examples

## 3. C# 14: Null-conditional assignment

- [x] 3.1 Add analyzer rule (new `SHARPENxxx`) to detect `if (x != null) x.Member = rhs;` single-statement patterns
- [x] 3.2 Implement safety checker: ensure condition receiver matches assignment receiver and body contains exactly one assignment
- [x] 3.3 Implement code fix: rewrite to `x?.Member = rhs;` (preserve trivia)
- [x] 3.4 Add tests: diagnostic triggers for single-statement if (with and without braces)
- [x] 3.5 Add tests: no diagnostic for multi-statement if bodies or mismatched receivers
- [ ] 3.6 Add tests: code fix output and formatting
- [ ] 3.7 Update docs: rule description and safety constraints

## 4. C# 14: nameof with unbound generic types

- [ ] 4.1 Add analyzer rule (new `SHARPENxxx`) to detect `nameof(Generic<...>)` and suggest `nameof(Generic<...>)` unbound form
- [ ] 4.2 Implement safety checker: ensure the `nameof` binds and remains a compile-time constant
- [ ] 4.3 Implement code fix: replace constructed generic type syntax with unbound generic type syntax
- [ ] 4.4 Add tests: `nameof(Dictionary<string,int>)` → `nameof(Dictionary<,>)`
- [ ] 4.5 Add tests: no diagnostic for non-generic `nameof`
- [ ] 4.6 Update docs: rule description and examples

## 5. C# 14: Lambda parameter modifiers without types

- [ ] 5.1 Add analyzer rule (new `SHARPENxxx`) to detect lambdas where parameter types are redundant and only present for modifiers
- [ ] 5.2 Implement safety checker: confirm target typing is unambiguous and converted delegate type is unchanged
- [ ] 5.3 Implement code fix: remove explicit parameter types while preserving modifiers
- [ ] 5.4 Add tests: safe contexts where target delegate type is known
- [ ] 5.5 Add tests: no diagnostic when overload resolution would change or binding is ambiguous
- [ ] 5.6 Update docs: rule description and examples

## 6. C# 14: Implicit Span/ReadOnlySpan conversions (redundant conversions)

- [ ] 6.1 Add analyzer rule (new `SHARPENxxx`) to detect redundant `AsSpan()` / explicit span conversions
- [ ] 6.2 Implement safety checker: ensure invoked symbol/overload is unchanged after removing conversion
- [ ] 6.3 Implement code fix: remove redundant conversion and preserve trivia
- [ ] 6.4 Add tests: redundant `AsSpan()` removal
- [ ] 6.5 Add tests: no diagnostic when conversion affects overload resolution
- [ ] 6.6 Update docs: rule description and safety constraints

## 7. C# 14: Extension members (extension blocks)

- [ ] 7.1 Add informational analyzer rule (new `SHARPENxxx`) suggesting extension blocks for static classes with many extension methods for the same receiver
- [ ] 7.2 (Optional) Implement limited code fix: refactor same-file extension methods into an extension block when purely mechanical
- [ ] 7.3 Implement safety checker for optional fix: reject cross-file/partial/preprocessor-complex cases
- [ ] 7.4 Add tests: diagnostic triggers and non-triggers
- [ ] 7.5 Add tests: code fix (if implemented)
- [ ] 7.6 Update docs: rule description and conservative scope

## 8. C# 14: Partial constructors and partial events (informational)

- [ ] 8.1 Add informational analyzer rule (new `SHARPENxxx`) for partial constructor patterns
- [ ] 8.2 Add informational analyzer rule (new `SHARPENxxx`) for partial event patterns
- [ ] 8.3 Add tests: diagnostics trigger on representative patterns
- [ ] 8.4 Update docs: guidance and caveats (no automatic fix)

## 9. C# 14: User-defined compound assignment operators (informational)

- [ ] 9.1 Add informational analyzer rule (new `SHARPENxxx`) suggesting compound assignment operators when `+=` is used and only binary operator exists
- [ ] 9.2 Add tests: diagnostics trigger on representative patterns
- [ ] 9.3 Update docs: guidance and caveats (no automatic fix)

## 10. Integration and quality gates

- [ ] 10.1 Ensure all new rules are registered in the rule catalog and exposed consistently
- [ ] 10.2 Ensure safety checker mapping is updated for all fixable rules
- [ ] 10.3 Run full test suite and fix failures
- [ ] 10.4 Add/refresh documentation index pages for C# 14 rules (and update any versioned docs like [`docs/csharp-13.md`](docs/csharp-13.md:1) if applicable)
