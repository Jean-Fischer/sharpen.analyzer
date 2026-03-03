# Tasks

## Discovery / Inventory

- [x] Inventory upstream C# 7.1 suggestions/analyzers under [`original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp71/`](original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp71/)
- [x] Identify any upstream shared helpers required by the C# 7.1 rules and map them to existing helpers in this repo (or plan minimal ports)
- [x] Decide sample folder naming (`csharp71` vs `csharp7_1`) and align with existing sample conventions in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/)

## Implementation

- [x] Port C# 7.1 analyzers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
- [x] Port C# 7.1 code fix providers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/) (where upstream provides fixes)
- [x] Register new diagnostics (descriptors, supported diagnostics, analyzer registration)
- [x] Add C# 7.1 samples under a new folder in the sample project

## Tests

- [x] Add analyzer tests for each migrated C# 7.1 rule in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/)
- [x] Add code fix tests for each migrated C# 7.1 rule that has a fix
- [x] Run full test suite and fix any failures

## Documentation / Spec Sync

- [x] Ensure the change artifacts are complete: [`openspec/changes/csharp-8/proposal.md`](openspec/changes/csharp-8/proposal.md), [`openspec/changes/csharp-8/design.md`](openspec/changes/csharp-8/design.md), and specs under [`openspec/changes/csharp-8/specs/`](openspec/changes/csharp-8/specs/)
- [ ] (Later) Sync finalized specs into [`openspec/specs/`](openspec/specs/) once implementation is complete
