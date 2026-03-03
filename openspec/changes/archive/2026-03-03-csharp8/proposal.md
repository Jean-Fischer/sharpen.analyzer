## Why

The legacy Sharpen project contains analyzers targeting C# 8 that are not yet available in this repository. Migrating them preserves existing value, reduces maintenance on the old codebase, and enables continued evolution of the analyzers in the new project.

## What Changes

- Inventory all C# 8 analyzers in the old project and map them to their equivalents (or new additions) in this repository.
- Port each analyzer implementation to the new project structure (diagnostic IDs, categories, messages, and supported language versions).
- Port associated code fixes (where applicable) and any shared helper utilities required by the analyzers.
- Port and/or rewrite unit tests to the new test project conventions.
- Ensure analyzers are packaged/registered consistently with the new project (NuGet/VSIX as applicable).
- Validate behavior parity with the old project and ensure the full test suite passes.

## Capabilities

### New Capabilities
- `csharp-8-analyzers-migration`: Migrate all analyzers (and related code fixes/tests) that target C# 8 from the legacy Sharpen project into this repository.

### Modified Capabilities
<!-- None. -->

## Impact

- Code:
  - Analyzer implementations under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
  - Code fix providers under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/)
  - Shared utilities under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/)
  - Tests under [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/)
- Dependencies:
  - Potential updates to Roslyn packages if the old analyzers rely on APIs not currently referenced.
- Build/CI:
  - Test suite expansion and potential baseline updates.
- Risk:
  - Behavior drift if diagnostics/code fixes differ; mitigated by porting tests and adding parity checks.