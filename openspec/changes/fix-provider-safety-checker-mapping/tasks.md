## 1. Discovery + Mapping Registry

- [ ] 1.1 Identify current fix provider discovery/registration mechanism(s) and where safety checks are currently invoked
- [ ] 1.2 Define the mapping key (fix provider type vs fix-provider id) and document the choice
- [ ] 1.3 Implement a central mapping registry (single source of truth) for fix provider → safety checker
- [ ] 1.4 Add validation to the registry to enforce one-to-one constraints (no missing mappings, no duplicates)

## 2. Integration Flow

- [ ] 2.1 Update the fix-application pipeline to resolve the safety checker exclusively via the mapping registry
- [ ] 2.2 Ensure the pipeline executes the mapped safety checker before applying the fix
- [ ] 2.3 Define and implement the failure mode for mapping/validation failures (exception/diagnostic/skip) and make it consistent
- [ ] 2.4 Ensure all fix-application entry points use the same mapping-based safety-check flow

## 3. Migrate Existing Fix Providers

- [ ] 3.1 Inventory existing fix providers and existing safety checkers
- [ ] 3.2 Add mapping entries for all existing fix providers
- [ ] 3.3 Resolve any cases where multiple fix providers share a checker by introducing a composite checker or splitting checkers

## 4. Tests + Guardrails

- [ ] 4.1 Add unit tests that assert the mapping is complete (every fix provider has exactly one checker)
- [ ] 4.2 Add unit tests that assert the mapping is one-to-one (no checker mapped to multiple fix providers)
- [ ] 4.3 Add integration tests that verify safety checks run before fixes and that failures prevent fix application

## 5. Documentation

- [ ] 5.1 Document the required steps to add a new fix provider + safety checker pair (where to register mapping, how to test)
- [ ] 5.2 Add troubleshooting guidance for common mapping validation failures
