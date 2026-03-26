## ADDED Requirements

### Requirement: Shared idempotency verification helper
The test suite SHALL provide a reusable helper that verifies a code fix is idempotent by applying the same code fix twice and asserting the second application produces no further changes.

#### Scenario: Idempotency holds for a code fix
- **WHEN** a test applies a code fix to an input source and captures the fixed output
- **THEN** applying the same code fix again to the fixed output produces identical source text

### Requirement: Helper integrates with existing code fix verification style
The idempotency helper SHALL integrate with the existing test infrastructure so that tests can continue to express expectations using the current “before/after” pattern.

#### Scenario: Test uses before/after and also asserts idempotency
- **WHEN** a test verifies a code fix using a before/after pair
- **THEN** the test can additionally assert idempotency without duplicating the code fix application logic

### Requirement: Incremental adoption across fix provider tests
The test suite SHALL allow incremental adoption of idempotency checks across code fix provider tests.

#### Scenario: Only a subset of tests use idempotency initially
- **WHEN** idempotency checks are introduced
- **THEN** existing tests can be updated gradually without requiring a repository-wide rewrite

### Requirement: Clear failure output on non-idempotent fixes
When a code fix is not idempotent, the helper SHALL fail the test with output that makes it clear what changed between the first and second application.

#### Scenario: Non-idempotent fix produces a readable diff signal
- **WHEN** the second application produces different source text than the first
- **THEN** the test failure message includes both versions (or a diff) to aid debugging
