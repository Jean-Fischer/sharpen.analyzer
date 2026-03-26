## Why

Code fix providers must be stable: applying a fix should converge to a final form, not keep changing the code on subsequent runs. Today, the test suite verifies a single application of each code fix, but it does not detect non-idempotent behavior (cascading replacements, unintended matches, or unstable syntax rewrites).

Adding idempotency tests will immediately increase confidence in all fix providers without changing analyzer/fix architecture.

## What Changes

- Add a shared test helper that applies a code fix twice and asserts the second pass produces no further changes.
- Apply this helper across existing code fix provider test suites (incrementally, starting with the most-used providers).
- Ensure the helper supports the existing test style (e.g., `VerifyCS.VerifyCodeFixAsync(before, after)`) and integrates cleanly with current test infrastructure.

## Capabilities

### New Capabilities
- `codefix-idempotency-tests`: Provide a reusable test helper and conventions to assert that every code fix provider is idempotent (applying the fix twice yields identical output).

### Modified Capabilities

- (none)

## Impact

- Affects test infrastructure under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/Infrastructure:1) and multiple code fix provider test files.
- No changes to production analyzer/fix provider behavior; only test coverage and helper APIs.
- May surface existing non-idempotent fixes, requiring follow-up fixes in specific code fix providers.