## Why

Sharpen.Analyzer currently has **two overlapping safety mechanisms**:

- [`FirstPassSafetyRunner`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafetyRunner.cs:1): a global, early gate intended to prevent unsafe fix computation.
- Fix-provider safety mapping + checkers (see [`FixProviderSafety`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/README.md:1)): a per-fix-provider gate used to suppress diagnostics and code actions when a fix is not safe.

This split creates ambiguity and drift:

- Some fix providers rely on the global first-pass gate, others rely on per-provider checkers, and some do both.
- The pipeline is harder to reason about and test because “what safety ran?” depends on the call site.
- Adding a new fix provider requires knowing which runner to call and where to plug safety.

We want **one safety pipeline** with a single entry point that is always used.

## What Changes

- Consolidate safety execution into a unified pipeline executed by **`FixProviderSafetyRunner`**.
- Make the pipeline two-stage:
  1. **Global FirstPassSafety gate** (conservative, cheap, cross-cutting)
  2. **Local per-fix-provider safety checker** via [`IFixProviderSafetyChecker`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/IFixProviderSafetyChecker.cs:1)
- Deprecate [`FirstPassSafetyRunner`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafetyRunner.cs:1) and migrate all call sites to use `FixProviderSafetyRunner`.
- Keep the existing fix-provider-to-checker mapping (and its validation) as the canonical registry.
- Add tests that lock the new end-to-end behavior (global gate + local checker) and prevent regressions.
- Update documentation to describe the unified pipeline and migration guidance.

## Capabilities

### New Capabilities

- `fix-provider-safety-pipeline-unification`: A single, explicit safety pipeline that always runs the global first-pass gate and then the mapped per-fix-provider checker.

### Modified Capabilities

- `fix-provider-safety-check-layer`: Clarify that the “first safety check” is not a separate runner anymore; it is a stage inside `FixProviderSafetyRunner`.
- `fix-provider-safety-checker-mapping`: Treat the mapping as the canonical source of per-provider safety logic, used by the unified runner.

## Impact

- **Fix providers:** all fix providers must call `FixProviderSafetyRunner` (directly or via a shared helper) instead of calling `FirstPassSafetyRunner` or performing ad-hoc safety.
- **Safety infrastructure:** `FixProviderSafetyRunner` becomes the single orchestration point; `FirstPassSafetyRunner` becomes deprecated and later removable.
- **Tests:** new tests for ordering, short-circuiting, and correct outcome propagation.
- **Docs:** update developer docs to explain the unified pipeline and how to add a new fix provider + checker.
