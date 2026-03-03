## Why

Sharpen.Analyzer currently includes migrations for earlier C# versions, but the C# 7.1 feature set from the upstream/original Sharpen repository has not yet been migrated into this analyzer project. Adding the C# 7.1 set keeps the analyzer suite aligned with upstream feature coverage and enables users to modernize codebases targeting C# 7.1.

## What Changes

- Migrate the C# 7.1 feature analyzers/code fixes (and any shared helpers they require) from the upstream/original Sharpen codebase into this repository.
- Add/port the C# 7.1 sample snippets into the sample project under a new `csharp71` folder (or equivalent), mirroring how previous language-version samples were integrated.
- Add/port unit tests for the migrated analyzers/code fixes, following the patterns used for the existing migrations.
- Ensure the migrated rules are wired into the analyzer package (diagnostic descriptors, registration, and documentation where applicable).
- No breaking changes expected (new rules/features only).

## Capabilities

### New Capabilities
- `csharp-8-migration`: Port C# 7.1 feature analyzers, code fixes, tests, and samples from the original Sharpen repository into the new Sharpen.Analyzer project structure.

### Modified Capabilities

<!-- None. This change adds new capabilities/rules; it does not change existing spec-level behavior. -->

## Impact

- New/updated code in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/) and [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/).
- New/updated tests in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/).
- New/updated samples in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/) (new `csharp71` folder).
- Potential need to port small shared utilities from upstream (e.g., syntax helpers) into the local `Common/` area.
