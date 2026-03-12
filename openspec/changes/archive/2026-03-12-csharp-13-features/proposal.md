## Why

C# 13 introduces several language and BCL features that can improve performance, safety, and readability, but they also introduce new patterns and migration opportunities that are easy to miss or apply unsafely. This change adds first-class Sharpen analyzers, safety checkers, and (where feasible) fix providers to help teams adopt the most valuable C# 13 features with confidence.

## What Changes

- Add new C# 13-focused diagnostics (primarily suggestion-level) for high-value, low-risk modernizations.
- Add fix providers for the subset of diagnostics where an automated transformation can be made safe and predictable.
- Add safety checkers for each fix provider to prevent unsafe transformations.
- Add unit tests (positive + negative) for each analyzer and fix provider.
- Update documentation to describe the new rules, their intent, and safety constraints.

## Capabilities

### New Capabilities
- `prefer-params-collections`: Detect `params T[]` that can be safely migrated to collection-based `params` (e.g., `ReadOnlySpan<T>`), and provide a solution-wide fix for non-public APIs when safe.
- `use-from-end-index-in-object-initializers`: Detect end-based index patterns in object/collection initializers and suggest using `^` indices; provide a fix for safe patterns.
- `use-escape-sequence-e`: Detect `\u001b` / `\x1b` escape usage and suggest `\e` where unambiguous; provide a fix.
- `use-system-threading-lock`: Detect dedicated private sync objects used only for `lock` and suggest migrating to `System.Threading.Lock`; provide a fix only when no `Monitor`-specific usage exists.
- `partial-properties-indexers-refactoring`: Detect opportunities to split/align property/indexer declarations into C# 13 partial property/indexer forms; provide a refactoring-style code action where feasible.
- `suggest-allows-ref-struct-constraint`: Detect generic APIs that could benefit from `allows ref struct` and provide guidance (analysis-first; fix optional and review-required).
- `suggest-overload-resolution-priority`: Detect overload sets where `OverloadResolutionPriorityAttribute` could reduce ambiguity and provide guidance (analysis-first; fix optional and review-required).

### Modified Capabilities
- (none)

## Impact

- New analyzers, fix providers, and safety checkers will be added under the existing C#-versioned organization (new C# 13 folder/namespace as needed).
- Test suite will expand with new analyzer and code-fix tests.
- Documentation will be updated (rule list + fix-provider safety guidance) to include the new C# 13 rules and their constraints.
- Some fixes (notably `params` signature changes) will require solution-wide updates and must be restricted to non-public APIs to avoid breaking changes.
