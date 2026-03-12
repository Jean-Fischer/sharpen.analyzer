## 1. Baseline / scaffolding

- [x] 1.1 Create C# 13 analyzer/fix-provider folder/namespace structure consistent with existing C# versioned rules
- [x] 1.2 Allocate SHARPEN diagnostic IDs and titles for the C# 13 rules in the central diagnostics catalog
- [x] 1.3 Add documentation stubs/entries for the new C# 13 rules in the docs site/readme

## 2. SHARPEN0XX Prefer params collections (prefer-params-collections)

- [x] 2.1 Implement analyzer: detect `params T[]` candidates and classify (public vs non-public)
- [x] 2.2 Implement safety checker: verify non-public symbol, target type availability, and safe body usage (no array-only semantics)
- [x] 2.3 Implement fix provider: change signature to supported collection params type and update in-solution call sites
- [x] 2.4 Add analyzer tests: positive + negative cases (public API, array semantics usage)
- [x] 2.5 Add fix-provider tests: safe transformation + blocked transformation cases
- [x] 2.6 Update documentation: intent, constraints, and examples

## 3. SHARPEN0XX Use from-end index in object initializers (use-from-end-index-in-object-initializers)

- [x] 3.1 Implement analyzer: detect end-based index patterns in initializers (e.g., `Length - 1`)
- [x] 3.2 Implement safety checker: ensure target supports `System.Index` indexing and pattern is provably equivalent
- [x] 3.3 Implement fix provider: replace with `^` index (`^1`, etc.)
- [x] 3.4 Add analyzer tests: positive + negative patterns
- [x] 3.5 Add fix-provider tests: correct rewrite + blocked ambiguous cases
- [x] 3.6 Update documentation: examples and limitations

## 4. SHARPEN0XX Use \e escape sequence (use-escape-sequence-e)

- [x] 4.1 Implement analyzer: detect `\u001b` and `\x1b` in string/char literals
- [x] 4.2 Implement safety checker: ensure `\x` replacement is unambiguous and preserves tokenization
- [x] 4.3 Implement fix provider: replace with `\e` where safe
- [x] 4.4 Add analyzer tests: string + char cases
- [x] 4.5 Add fix-provider tests: safe replacements + blocked ambiguous `\x` cases
- [x] 4.6 Update documentation: examples and caveats

## 5. SHARPEN0XX Use System.Threading.Lock (use-system-threading-lock)

- [x] 5.1 Implement analyzer: detect dedicated private sync fields used only in `lock` statements
- [x] 5.2 Implement safety checker: ensure no `Monitor.*` usage and no non-lock usages; ensure `System.Threading.Lock` is resolvable
- [x] 5.3 Implement fix provider: change field type/initializer to `System.Threading.Lock` and keep `lock(field)` usage
- [x] 5.4 Add analyzer tests: positive + negative (field used elsewhere, Monitor usage)
- [x] 5.5 Add fix-provider tests: safe migration + blocked migration
- [ ] 5.6 Update documentation: intent, framework requirements, and constraints

## 6. SHARPEN0XX Partial properties/indexers refactoring (partial-properties-indexers-refactoring)

- [x] 6.1 Implement analyzer: detect eligible property/indexer patterns in partial types
- [x] 6.2 Implement safety checker: validate signature match and that produced partial member is valid
- [x] 6.3 Implement code action: generate declaring + implementing partial declarations (refactoring-style)
- [ ] 6.4 Add analyzer tests: eligible + ineligible patterns
- [ ] 6.5 Add code-action tests: produced code compiles and preserves behavior
- [ ] 6.6 Update documentation: usage guidance and limitations

## 7. SHARPEN0XX Suggest allows ref struct constraint (suggest-allows-ref-struct-constraint)

- [ ] 7.1 Implement analyzer: detect candidate generic APIs and emit guidance-only diagnostic
- [ ] 7.2 (Optional) Implement review-required code action: insert `allows ref struct` constraint
- [ ] 7.3 Add tests: diagnostic presence + absence; code action (if implemented)
- [ ] 7.4 Update documentation: explain ref-safety implications and review requirement

## 8. SHARPEN0XX Suggest OverloadResolutionPriorityAttribute (suggest-overload-resolution-priority)

- [ ] 8.1 Implement analyzer: detect overload sets likely to benefit and emit guidance-only diagnostic
- [ ] 8.2 (Optional) Implement review-required code action: add `OverloadResolutionPriorityAttribute` with suggested priority
- [ ] 8.3 Add tests: diagnostic presence + absence; code action (if implemented)
- [ ] 8.4 Update documentation: explain intended use for library authors and review requirement

## 9. Documentation and integration

- [ ] 9.1 Update rule index / README to include C# 13 rules and their IDs
- [ ] 9.2 Update fix-provider safety documentation to include the new safety checkers and their gating rules
- [ ] 9.3 Ensure all new analyzers/fix providers are wired into the package exports and test discovery
- [ ] 9.4 Run full test suite and fix any build/test failures
