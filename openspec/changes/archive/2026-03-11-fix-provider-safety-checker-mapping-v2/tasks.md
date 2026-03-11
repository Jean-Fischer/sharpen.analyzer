## 1. Safety infrastructure and base types

- [x] 1.1 Create `Safety/` folder structure for per-fix safety checkers and shared base types
- [x] 1.2 Define base safety checker interface/contract (inputs, safe/unsafe outcome, reason)
- [x] 1.3 Add shared helpers/utilities for common syntax/semantic preconditions

## 2. FixProvider ↔ SafetyChecker mapping

- [x] 2.1 Implement central mapping registry (fix provider type → safety checker type)
- [x] 2.2 Add validation that enforces one-to-one mapping (no duplicates)
- [x] 2.3 Add validation that enforces completeness (no missing fix providers)
- [ ] 2.4 Document the canonical mapping table in code and in docs

## 3. Implement safety checkers (initial set)

- [x] 3.1 Implement `NullCheckSafetyChecker`
- [x] 3.2 Implement `CollectionExpressionSafetyChecker`
- [x] 3.3 Implement `StringInterpolationSafetyChecker`
- [x] 3.4 Implement `SwitchExpressionSafetyChecker`
- [x] 3.5 Implement `LinqSafetyChecker`

## 4. Pipeline integration (diagnostics + code actions)

- [x] 4.1 Update analyzer pipeline to consult mapped safety checker before reporting diagnostics
- [x] 4.2 Update fix provider pipeline to consult mapped safety checker before registering code actions
- [x] 4.3 Ensure analyzer and fix provider paths share the same safety evaluation result shape

## 5. Integration flow example (NullCheck)

- [x] 5.1 Add an end-to-end example flow: `NullCheckAnalyzer` → `NullCheckSafetyChecker` → `NullCheckFixProvider`
- [x] 5.2 Add a small developer doc section explaining how to add a new fix provider + safety checker pair

## 6. Tests

- [x] 6.1 Add unit tests for mapping validation (missing entry, duplicate entry)
- [x] 6.2 Add unit tests for each safety checker (safe and unsafe scenarios)
- [x] 6.3 Add integration tests verifying diagnostics are suppressed when unsafe
- [x] 6.4 Add integration tests verifying code actions are suppressed when unsafe

## 7. Documentation

- [x] 7.1 Update docs to include the mapping summary table (NullCheck, CollectionExpression, StringInterpolation, SwitchExpression, Linq)
- [x] 7.2 Update docs to describe the safety gate timing (before diagnostic reporting and before code actions)
