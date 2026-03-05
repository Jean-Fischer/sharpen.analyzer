Main folders
Sharpen.Analyzer/: the new Roslyn-analyzer solution and projects.

Sharpen.Analyzer/Sharpen.Analyzer.sln: solution containing:
Sharpen.Analyzer (main analyzer project)
Sharpen.Analyzer.Tests (unit tests for analyzers/code fixes)
Sharpen.Analyzer.Sample (manual reproduction playground)
Sharpen.Analyzer/Sharpen.Analyzer/: analyzer implementation (currently includes sample analyzers/code fixes such as SampleSemanticAnalyzer and SampleCodeFixProvider).
Sharpen.Analyzer/Sharpen.Analyzer.Tests/: analyzer/code-fix tests (e.g. UseVarKeywordAnalyzerTests, UseVarKeywordCodeFixTests).
original-sharpen/: the legacy, unsupported Visual Studio extension codebase used as reference.

Contains the original engine and suggestion implementations under original-sharpen/src/, including many “SharpenSuggestions” grouped by C# version (e.g. original-sharpen/src/Sharpen.Engine/SharpenSuggestions/).
Also includes docs and historical context like original-sharpen/README.md and original-sharpen/CHANGELOG.md.
openspec/: OpenSpec workflow folder used to manage artifact-driven changes.

## C# 11 rules

Implemented in the OpenSpec change at `openspec/changes/csharp-11/`.

| Rule ID | Title | Feature | Notes |
|---|---|---|---|
| SHARPEN046 | Use raw string literal | Raw string literals | Code fix available |
| SHARPEN047 | Use required member | Required members | Code fix available |
| SHARPEN048 | Use generic math constraints | Generic math | Analyzer-only guidance |
| SHARPEN049 | Use list pattern | Span/array list-pattern matching | Analyzer available; code fix is currently a no-op due to Roslyn version limitations |
| SHARPEN050 | Use UTF-8 string literal | UTF-8 string literals | Code fix available |

A change was scaffolded at `openspec/changes/document-current-state/` (schema: spec-driven). No artifacts were written yet.

Key intent captured from this discussion:
- Clarify what the “core” project is (the Roslyn analyzer under `Sharpen.Analyzer/`) and how it relates to the legacy reference implementation (`original-sharpen/`).
- Use `original-sharpen/` as an inspiration/source of truth for what scans/suggestions exist, while acknowledging the product shift (VS extension → Roslyn analyzer) changes how they must be implemented, packaged, and tested.
