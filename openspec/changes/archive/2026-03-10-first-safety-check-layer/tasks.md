## 1. Pipeline contract and types

- [x] 1.1 Identify the current match → fix pipeline entry point(s) in the fix provider infrastructure
- [x] 1.2 Define a first-pass safety check interface (inputs: document/semantic model/match result; output: structured pass/fail + reason)
- [x] 1.3 Define a safety check result type (e.g., Safe/Unsafe with reason id + optional message)
- [x] 1.4 Define how multiple checks are registered and the deterministic evaluation order

## 2. Integrate first safety check gate

- [x] 2.1 Insert the first safety check gate after match candidate creation and before fix generation
- [x] 2.2 Ensure the pipeline short-circuits on first failure (no fix generated, no edits)
- [x] 2.3 Add optional internal logging hook for unsafe outcomes (include reason id)
- [x] 2.4 Ensure cancellation is respected throughout safety check evaluation

## 3. Update existing safety-check layer behavior

- [x] 3.1 Align existing safety-check layer implementation with the new “first-pass gate” semantics
- [x] 3.2 Ensure “match failed” vs “match succeeded but unsafe” are represented as distinct outcomes

## 4. Tests

- [x] 4.1 Add unit tests for deterministic ordering and stop-at-first-failure behavior
- [x] 4.2 Add at least one integration-style test where a match is found but the first safety check blocks fix generation
- [x] 4.3 Add tests asserting the unsafe outcome is observable without producing edits

## 5. Documentation and cleanup

- [x] 5.1 Update developer docs/comments near the fix pipeline to describe the new stage
- [x] 5.2 Ensure all new public APIs are documented and named consistently with existing conventions
