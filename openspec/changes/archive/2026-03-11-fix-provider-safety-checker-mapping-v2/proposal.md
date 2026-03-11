## Why

Fix providers currently decide their own “safety” ad-hoc (or not at all), which risks offering code actions that are invalid for the current syntax/semantic context and makes it hard to reason about when a fix is safe to apply.

We need a consistent, explicit, and testable safety gate that runs before diagnostics are reported and before code actions are offered.

## What Changes

- Introduce a one-to-one mapping between each fix provider and a dedicated safety checker.
- Run the safety checker before:
  - the analyzer reports the diagnostic, and
  - the fix provider offers a code action.
- Standardize how safety is evaluated and surfaced (clear outcomes, consistent logging/telemetry hooks if needed).
- Add documentation and tests that demonstrate the integration flow and enforce the mapping.

## Capabilities

### New Capabilities

- `fix-provider-safety-checker-mapping-v2`: Define and enforce a one-to-one mapping between fix providers and safety checkers, including a canonical mapping table and validation rules.
- `fix-provider-safety-checker-integration-flow-v2`: Define the end-to-end pipeline flow showing where safety checkers run relative to analyzers and fix providers.

### Modified Capabilities

- `fix-provider-safety-check-layer`: Extend/clarify the existing safety check layer requirements to support per-fix-provider checkers and to ensure the safety gate is applied consistently in both analyzer and code-fix paths.

## Impact

- Analyzer pipeline: diagnostics emission must be gated by the relevant safety checker.
- Fix provider pipeline: code action registration must be gated by the same safety checker.
- New/updated safety infrastructure: base types, registration/mapping, and shared evaluation utilities.
- Tests: new unit/integration tests to validate mapping completeness and to validate the analyzer+fix-provider flow.
- Documentation: update developer docs to explain how to add a new fix provider + safety checker pair.