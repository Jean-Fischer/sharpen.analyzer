## Why

The current fix-provider implementation is spread across multiple assemblies/namespaces in a way that makes it hard to reason about ownership, dependencies, and safety-pipeline responsibilities.
This change proposes splitting and clarifying fix-provider “providers” vs “assembly” responsibilities to improve maintainability and reduce coupling.

## What Changes

- Introduce a clearer separation between:
  - “provider” concerns (registration, discovery, metadata)
  - “assembly” concerns (Roslyn code fix provider implementation and wiring)
- Move/rename types so that fix-provider code is grouped by responsibility rather than by historical location.
- Update internal references and any public surface area impacted by the move/rename.
- Ensure the safety-check pipeline continues to work with the new structure.

## Capabilities

### New Capabilities
- `split-fix-providers-assembly`: Define the required structure and boundaries between fix-provider provider-layer and assembly-layer code, including naming and dependency rules.

### Modified Capabilities
- `fix-provider-safety-check-layer`: Adjust requirements (if needed) to reflect the new layering boundaries and where safety checks are applied.

## Impact

- Affected code:
  - Fix provider implementations under `Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/**`
  - Any registration/discovery code for fix providers
  - Safety-check pipeline integration points
- Potential impact on:
  - Internal APIs and type names (moves/renames)
  - Build/packaging if assemblies/projects are split or reorganized
