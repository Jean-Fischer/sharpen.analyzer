## Why

C# 14 introduces several language features that can simplify code, improve performance, and reduce boilerplate. Adding first-class support in Sharpen (analyzers + safety checkers + fix providers + tests + docs) lets teams adopt these features safely and consistently, while keeping fixes conservative and reviewable.

## What Changes

- Add a new set of C# 14-focused rules (analyzer + safety checker + code fix when feasible) for a curated subset of features.
- Provide unit test coverage for each new rule (diagnostic + code fix where applicable).
- Update documentation to describe each rule, its safety constraints, and examples.
- Rename/renumber rule IDs from the legacy `EDRxxxx` prefix to `SHARPENxxxx`, starting at `SHARPEN001` and incrementing based on the existing project’s current highest `SHARPEN` rule.

## Capabilities

### New Capabilities
- `csharp-14-extension-members`: Suggest/assist organizing extension methods into C# 14 extension blocks (conservative, mostly informational; limited fix).
- `csharp-14-field-backed-properties`: Convert eligible manual backing-field properties to field-backed properties using the `field` keyword (safe fix when field is private and unused elsewhere).
- `csharp-14-implicit-span-conversions`: Detect redundant explicit span conversions (e.g., unnecessary `AsSpan()`), with safe fixes to remove redundancy.
- `csharp-14-nameof-unbound-generics`: Suggest using unbound generic types in `nameof` when a closed generic is used only to obtain the type name.
- `csharp-14-lambda-parameter-modifiers`: Suggest simplifying lambda parameter declarations by removing explicit types when only needed for modifiers and the target delegate type is unambiguous.
- `csharp-14-partial-constructors-events`: Informational guidance analyzers for partial constructors/events patterns (no automatic fix).
- `csharp-14-compound-assignment-operators`: Informational analyzer suggesting user-defined compound assignment operators for performance-sensitive types (no automatic fix).
- `csharp-14-null-conditional-assignment`: Convert simple guarded assignments to null-conditional assignment (safe fix for single-statement `if (x != null) x.Member = rhs;`).
- `rule-id-migration-edr-to-sharpen`: Migrate rule IDs from `EDR` to `SHARPEN` and ensure consistent numbering across analyzers, fix providers, tests, and docs.

### Modified Capabilities
- (none)

## Impact

- New analyzers and fix providers in the Sharpen analyzer assemblies, plus new safety checkers integrated into the existing fix-provider safety pipeline.
- New/updated unit tests in the test project(s) for each rule.
- Documentation updates under [`docs/`](docs:1) and potentially the main [`Readme.md`](Readme.md:1) to list new rules and their safety constraints.
- Rule ID renumbering touches diagnostic descriptors, rule catalogs, documentation pages, and test baselines; requires careful coordination to avoid breaking existing references.