## Why

Today, fix providers and safety checkers are not enforced as a strict one-to-one pair, which makes it easy to ship a code fix without the corresponding safety validation (or to run a safety check that is not clearly tied to a fix). This change makes the relationship explicit so the pipeline can reliably validate that every fix is guarded by the intended safety checks.

## What Changes

- Introduce an explicit one-to-one mapping between each fix provider and its safety checker.
- Define a single integration flow that uses the mapping to:
  - discover fix providers,
  - resolve the corresponding safety checker,
  - run the safety checker before applying the fix.
- Add validation that the mapping is complete and unambiguous (e.g., no fix provider without a checker, no checker mapped to multiple fix providers).
- Update registration/discovery so new fix providers must declare their safety checker pairing.

## Capabilities

### New Capabilities
- `fix-provider-safety-checker-mapping`: Define and enforce a one-to-one mapping between fix providers and safety checkers, and expose it to the integration flow.
- `fix-provider-safety-checker-integration-flow`: Standardize the runtime flow that uses the mapping to run safety checks before applying fixes.

### Modified Capabilities
- `fix-provider-safety-check-layer`: Tighten requirements so safety checks are resolved via the mapping and are mandatory for all fix providers.

## Impact

- Affects fix provider registration/discovery and the safety-check execution pipeline.
- May require updating existing fix providers to declare their mapped safety checker.
- Adds new validation errors/fail-fast behavior when the mapping is incomplete or inconsistent.
