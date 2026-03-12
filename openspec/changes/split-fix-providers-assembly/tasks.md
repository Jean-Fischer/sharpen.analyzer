## 1. Baseline & inventory

- [x] 1.1 Identify all fix-provider related folders/namespaces/projects currently in use (provider/discovery vs concrete `CodeFixProvider` implementations)
- [x] 1.2 Identify current discovery/registration entry points and how fix providers are surfaced to the safety pipeline
- [x] 1.3 Identify any public APIs (if any) that would be impacted by moving/renaming types

## 2. Define target structure (provider vs assembly)

- [x] 2.1 Decide the target folder/namespace layout for provider-layer code vs assembly-layer code
- [x] 2.2 Define the dependency direction rules (what provider layer may reference; what assembly layer may reference)
- [x] 2.3 Add lightweight documentation/comments in code to describe the layering model

## 3. Refactor: move/rename to match responsibilities

- [x] 3.1 Move provider-layer types (discovery/registration/metadata) into the provider-layer location
- [x] 3.2 Move concrete Roslyn `CodeFixProvider` implementations into the assembly-layer location
- [x] 3.3 Update namespaces/usings and fix compilation errors after moves
- [x] 3.4 Ensure no provider-layer code references concrete fix implementation types

## 4. Safety pipeline integration

- [x] 4.1 Update safety pipeline integration points to use provider-layer metadata/discovery after the refactor
- [x] 4.2 Ensure safety-check execution does not require assembly-layer references
- [x] 4.3 Add/adjust tests to cover discovery + safety pipeline execution for at least one representative fix provider

## 5. Validation & cleanup

- [x] 5.1 Run full test suite and fix any failures caused by the refactor
- [x] 5.2 Validate that each diagnostic that previously had a fix still has a fix after the refactor
- [x] 5.3 Remove dead code/obsolete paths and ensure documentation is up to date
