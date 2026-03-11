## 1. Define the unified safety pipeline contract

- [x] 1.1 Document the unified pipeline stages (global FirstPassSafety then local per-provider checker) and the short-circuit rules
- [x] 1.2 Define/confirm the result shape returned by `FixProviderSafetyRunner` (safe vs unsafe + reason, and whether the failure came from global or local stage)

## 2. Consolidate runners (design-level changes, no behavior drift)

- [x] 2.1 Move/compose the existing FirstPassSafety logic into `FixProviderSafetyRunner` as the global stage
- [x] 2.2 Deprecate [`FirstPassSafetyRunner`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafetyRunner.cs:1) (doc note + compiler warning/obsolete attribute if applicable)

## 3. Enforce single entry point adoption

- [x] 3.1 Identify all call sites that currently invoke `FirstPassSafetyRunner` and plan updates to route through `FixProviderSafetyRunner`
- [x] 3.2 Update all fix providers to call `FixProviderSafetyRunner` (directly or via a shared helper) before computing fixes / registering code actions
- [x] 3.3 Ensure analyzer diagnostic emission path also uses `FixProviderSafetyRunner` (same safety decision gates both diagnostics and code actions)

## 4. Keep and validate the mapping

- [x] 4.1 Keep the fix-provider-to-checker mapping as the canonical registry
- [x] 4.2 Extend mapping validation tests to ensure:
  - every fix provider is mapped
  - no duplicates
  - (optional) no unmapped checkers

## 5. Tests

- [x] 5.1 Add unit tests for `FixProviderSafetyRunner` ordering:
  - global unsafe → local checker not evaluated
  - global safe + local unsafe → unsafe
  - global safe + local safe → safe
- [x] 5.2 Add regression tests that ensure no fix provider calls `FirstPassSafetyRunner` (or that it is only used by the unified runner)
- [x] 5.3 Add representative integration tests for at least one provider demonstrating:
  - diagnostics suppressed when unsafe
  - code actions suppressed when unsafe

## 6. Documentation

- [x] 6.1 Update safety docs to describe the unified pipeline and the two-stage evaluation
- [x] 6.2 Update developer guidance for adding a new fix provider:
  - add checker implementing [`IFixProviderSafetyChecker`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/IFixProviderSafetyChecker.cs:1)
  - add mapping entry
  - rely on `FixProviderSafetyRunner` (do not call `FirstPassSafetyRunner`)
- [x] 6.3 Add a deprecation note and migration guidance for `FirstPassSafetyRunner`
