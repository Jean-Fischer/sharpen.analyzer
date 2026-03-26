## 1. Baseline and helper design

- [ ] 1.1 Review existing code fix test infrastructure and identify the minimal API surface needed to apply a code fix and capture the resulting source text (single pass).
- [ ] 1.2 Define the idempotency helper API (method name, parameters, overloads) to fit existing patterns like `VerifyCS.VerifyCodeFixAsync(before, after)`.

## 2. Implement idempotency helper

- [ ] 2.1 Implement a helper that applies the code fix once and returns the fixed source text (e.g., `ApplyFixAsync(string input)`), reusing existing verifier infrastructure.
- [ ] 2.2 Implement `VerifyIdempotent...Async` that applies the fix twice and asserts `fixedOnce == fixedTwice`.
- [ ] 2.3 Ensure failure output is actionable (include both versions and/or a diff-friendly message).
- [ ] 2.4 Add unit tests for the helper itself (at least one passing idempotent case and one failing non-idempotent case).

## 3. Adopt helper in existing code fix tests

- [ ] 3.1 Update a small representative set of existing code fix provider tests to use the idempotency helper (start with the most-used providers).
- [ ] 3.2 Run the test suite and fix any newly exposed non-idempotent behavior in the affected code fix providers (or temporarily quarantine with a documented opt-out if absolutely necessary).
- [ ] 3.3 Expand adoption to additional code fix provider test suites until coverage is satisfactory.

## 4. Documentation and guardrails

- [ ] 4.1 Document the idempotency testing convention in the test infrastructure (how to use the helper, when to opt out, and expected failure modes).
- [ ] 4.2 Add a lightweight checklist item/template note for new code fix providers to include an idempotency assertion in their tests.
