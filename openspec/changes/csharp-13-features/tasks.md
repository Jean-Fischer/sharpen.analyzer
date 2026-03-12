## 1. Baseline / scaffolding

- [ ] 1.1 Create C# 13 analyzer/fix-provider folder/namespace structure consistent with existing C# versioned rules
- [ ] 1.2 Add diagnostic IDs and titles for the C# 13 rules (EDR1001–EDR1007) in the central diagnostics catalog
- [ ] 1.3 Add documentation stubs/entries for the new C# 13 rules in the docs site/readme

## 2. EDR1001 Prefer params collections (prefer-params-collections)

- [ ] 2.1 Implement analyzer: detect `params T[]` candidates and classify (public vs non-public)
- [ ] 2.2 Implement safety checker: verify non-public symbol, target type availability, and safe body usage (no array-only semantics)
- [ ] 2.3 Implement fix provider: change signature to supported collection params type and update in-solution call sites
- [ ] 2.4 Add analyzer tests: positive + negative cases (public API, array semantics usage)
- [ ] 2.5 Add fix-provider tests: safe transformation + blocked transformation cases
- [ ] 2.6 Update documentation: intent, constraints, and examples

## 3. EDR1002 Use from-end index in object initializers (use-from-end-index-in-object-initializers)

- [ ] 3.1 Implement analyzer: detect end-based index patterns in initializers (e.g., `Length - 1`)
- [ ] 3.2 Implement safety checker: ensure target supports `System.Index` indexing and pattern is provably equivalent
- [ ] 3.3 Implement fix provider: replace with `^` index (`^1`, etc.)
- [ ] 3.4 Add analyzer tests: positive + negative patterns
- [ ] 3.5 Add fix-provider tests: correct rewrite + blocked ambiguous cases
- [ ] 3.6 Update documentation: examples and limitations

## 4. EDR1003 Use \e escape sequence (use-escape-sequence-e)

- [ ] 4.1 Implement analyzer: detect `\u001b` and `\x1b` in string/char literals
- [ ] 4.2 Implement safety checker: ensure `\x` replacement is unambiguous and preserves tokenization
- [ ] 4.3 Implement fix provider: replace with `\e` where safe
- [ ] 4.4 Add analyzer tests: string + char cases
- [ ] 4.5 Add fix-provider tests: safe replacements + blocked ambiguous `\x` cases
- [ ] 4.6 Update documentation: examples and caveats

## 5. EDR1004 Use System.Threading.Lock (use-system-threading-lock)

- [ ] 5.1 Implement analyzer: detect dedicated private sync fields used only in `lock` statements
- [ ] 5.2 Implement safety checker: ensure no `Monitor.*` usage and no non-lock usages; ensure `System.Threading.Lock` is resolvable
- [ ] 5.3 Implement fix provider: change field type/initializer to `System.Threading.Lock` and keep `lock(field)` usage
- [ ] 5.4 Add analyzer tests: positive + negative (field used elsewhere, Monitor usage)
- [ ] 5.5 Add fix-provider tests: safe migration + blocked migration
- [ ] 5.6 Update documentation: intent, framework requirements, and constraints

## 6. EDR1005 Partial properties/indexers refactoring (partial-properties-indexers-refactoring)

- [ ] 6.1 Implement analyzer: detect eligible property/indexer patterns in partial types
- [ ] 6.2 Implement safety checker: validate signature match and that produced partial member is valid
- [ ] 6.3 Implement code action: generate declaring + implementing partial declarations (refactoring-style)
- [ ] 6.4 Add analyzer tests: eligible + ineligible patterns
- [ ] 6.5 Add code-action tests: produced code compiles and preserves behavior
- [ ] 6.6 Update documentation: usage guidance and limitations

## 7. EDR1006 Suggest allows ref struct constraint (suggest-allows-ref-struct-constraint)

- [ ] 7.1 Implement analyzer: detect candidate generic APIs and emit guidance-only diagnostic
- [ ] 7.2 (Optional) Implement review-required code action: insert `allows ref struct` constraint
- [ ] 7.3 Add tests: diagnostic presence + absence; code action (if implemented)
- [ ] 7.4 Update documentation: explain ref-safety implications and review requirement

## 8. EDR1007 Suggest OverloadResolutionPriorityAttribute (suggest-overload-resolution-priority)

- [ ] 8.1 Implement analyzer: detect overload sets likely to benefit and emit guidance-only diagnostic
- [ ] 8.2 (Optional) Implement review-required code action: add `OverloadResolutionPriorityAttribute` with suggested priority
- [ ] 8.3 Add tests: diagnostic presence + absence; code action (if implemented)
- [ ] 8.4 Update documentation: explain intended use for library authors and review requirement

## 9. Documentation and integration

- [ ] 9.1 Update rule index / README to include C# 13 rules and their IDs
- [ ] 9.2 Update fix-provider safety documentation to include the new safety checkers and their gating rules
- [ ] 9.3 Ensure all new analyzers/fix providers are wired into the package exports and test discovery
- [ ] 9.4 Run full test suite and fix any build/test failures
