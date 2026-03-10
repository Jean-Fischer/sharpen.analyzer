## Why

Some code fixes can produce incorrect or unsafe edits when the pattern match is ambiguous or when required semantic preconditions are not met. We need a consistent “first safety check” layer between pattern matching and applying code fixes to prevent unsafe transformations and to make fix behavior predictable.

## What Changes

- Introduce a first-pass safety-check step that runs after a pattern match is found but before a code fix is computed/applied.
- Standardize how safety checks are expressed and evaluated (pass/fail + reason), so fixes can bail out early without producing edits.
- Ensure the fix pipeline can surface “not safe to apply” outcomes deterministically (no edits) and optionally with diagnostics/logging hooks.

## Capabilities

### New Capabilities
- `first-safety-check-layer`: Add a safety-check stage between pattern matching and code fix application, with a standard contract for checks and outcomes.

### Modified Capabilities
- `fix-provider-safety-check-layer`: Extend the existing safety-check layer requirements to include an explicit first-pass gate between match and fix generation.

## Impact

- Affects the fix provider pipeline (pattern matching → safety checks → fix generation/application).
- May require updates to existing fix providers to register/implement first-pass checks.
- May require updates to tests to cover “match found but not safe” scenarios.
