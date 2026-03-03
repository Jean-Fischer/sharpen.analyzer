## Why

Sharpen.Analyzer currently includes C# 6 feature analyzers/samples, but the C# 7 feature set from the original Sharpen repository has not yet been migrated into this new project structure. Adding the C# 7 set keeps the analyzer suite aligned with the upstream feature coverage and enables users to modernize codebases targeting C# 7.

## What Changes

- Migrate the C# 7 feature analyzers/code fixes (and any shared helpers they require) from the upstream/original Sharpen codebase into this repository.
- Add/port the C# 7 sample snippets into the sample project under a new `csharp7` folder, mirroring how C# 6 samples were integrated.
- Add/port unit tests for the migrated analyzers/code fixes, following the patterns used for the existing C# 6 migrations.
- Ensure the migrated rules are wired into the analyzer package (diagnostic descriptors, registration, and documentation where applicable).
- No breaking changes expected (new rules/features only).

## Capabilities

### New Capabilities
- `csharp-7-migration`: Port C# 7 feature analyzers, code fixes, tests, and samples from the original Sharpen repository into the new Sharpen.Analyzer project structure.

### Modified Capabilities

<!-- None. This change adds new capabilities/rules; it does not change existing spec-level behavior. -->

## Impact

- New/updated code in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/) and [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/).
- New/updated tests in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/).
- New/updated samples in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/) (new `csharp7` folder).
- Potential need to port small shared utilities from upstream (e.g., syntax helpers) into the local `Common/` area.
