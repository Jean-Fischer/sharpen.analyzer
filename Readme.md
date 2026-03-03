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

A change was scaffolded at openspec/changes/document-current-state/ (schema: spec-driven). No artifacts were written yet.
Key intent captured from this discussion
Clarify what the “core” project is (the Roslyn analyzer under Sharpen.Analyzer/) and how it relates to the legacy reference implementation (original-sharpen/).
Use original-sharpen/ as an inspiration/source of truth for what scans/suggestions exist, while acknowledging the product shift (VS extension → Roslyn analyzer) changes how they must be implemented, packaged, and tested.