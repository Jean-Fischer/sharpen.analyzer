# Project Instructions

## What this repo is
- `sharpen.analyzer` is a Roslyn analyzer package with optional code fixes for modernizing C# codebases.
- The solution is `Sharpen.Analyzer/Sharpen.Analyzer.sln`.
- The source tree lives under `Sharpen.Analyzer/Sharpen.Analyzer/`.
- Main projects:
  - `Sharpen.Analyzer` — analyzer assembly
  - `Sharpen.Analyzer.FixProviders` — code-fix assembly
  - `Sharpen.Analyzer.Core` — shared linked source used by the analyzer and fix providers
  - `Sharpen.Analyzer.Tests` — xUnit/Roslyn tests
  - `Sharpen.Analyzer.Sample` — sample code that intentionally triggers analyzers
- User-facing documentation starts in `Readme.md`; deeper guides live in `docs/`.

## Setup / install
- Use the .NET 10 SDK (`10.0.x` in CI).
- Package versions are managed centrally in `Sharpen.Analyzer/Directory.Packages.props`.
- Install the package you need with one of the documented commands:
  - `dotnet add package Sharpen.Analyzer`
  - `dotnet add package Sharpen.Analyzer.FixProviders`
- There is no repo-specific tool manifest or alternate package manager.

## Common commands
- Restore: `dotnet restore Sharpen.Analyzer/Sharpen.Analyzer.sln`
- Build: `dotnet build Sharpen.Analyzer/Sharpen.Analyzer.sln --configuration Release --no-restore`
- Test: `dotnet test Sharpen.Analyzer/Sharpen.Analyzer.sln --configuration Release --no-build --verbosity normal`
- Single test: `dotnet test Sharpen.Analyzer/Sharpen.Analyzer.sln --configuration Release --no-build --filter FullyQualifiedName~UseVarKeywordCodeFixTests`
- Bulk analyzer fixes: `dotnet format analyzers "Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/Sharpen.Analyzer.Sample.csproj" --verbosity detailed --severity info`
- Pack release packages: `dotnet pack "Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.csproj" --configuration Release --no-build --output nupkgs` and `dotnet pack "Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/Sharpen.Analyzer.FixProviders.csproj" --configuration Release --no-build --output nupkgs`
- CI parity for coverage: `dotnet test Sharpen.Analyzer/Sharpen.Analyzer.sln --configuration Release --no-build --collect:"XPlat Code Coverage;Format=opencover" --results-directory Sharpen.Analyzer/TestResults`

## Testing and verification
- Match CI by using Release builds/tests unless you have a strong reason not to.
- Tests in `Sharpen.Analyzer.Tests` disable parallelization because they mutate global static state.
- If you need coverage for SonarQube or similar checks, keep the results under `Sharpen.Analyzer/TestResults/`.
- Prefer verifying analyzer changes with both build and test runs, not just one or the other.

## Code / file conventions
- Analyzer rules and descriptors live in `Sharpen.Analyzer/Sharpen.Analyzer/Rules/`.
- Shared helpers live in `Common/`, `Extensions/`, `Helpers/`, and `Safety/` under the analyzer source root.
- Fix providers live in `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/FixProvider/`.
- Keep analyzer, fix provider, safety checker, and tests aligned for the same rule change.
- When diagnostics change, update `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md` as appropriate.
- When user-visible behavior changes, update `Readme.md` and the relevant file in `docs/`.
- Keep the analyzer assembly free of `Microsoft.CodeAnalysis.Workspaces`; only fix providers may use Workspaces APIs.
- The sample project is intentionally set up to trigger analyzers and has `RunAnalyzersDuringBuild=false` on purpose.
- Follow the existing xUnit naming pattern: test class names usually mirror the rule or fix they cover.
- Use the central package versions in `Directory.Packages.props`; do not add ad hoc package versions in individual project files.

## Safety / avoid rules
- Do not commit generated or local-only outputs: `bin/`, `obj/`, `TestResults/`, `*.nupkg`, `*.log`, `testlog*.txt`.
- Do not edit the vendored `original-sharpen/` tree unless the task explicitly targets it.
- Do not introduce `Microsoft.CodeAnalysis.Workspaces` into `Sharpen.Analyzer`.
- Do not enable analyzer execution in the sample project just to make its warnings disappear.
- Do not parallelize tests that depend on shared static state.
- Keep local IDE folders and settings out of commits, including `.idea/`, `.kilocode/`, and `.vscode/settings.json`.

## Notes for working in this repo
- `Readme.md` is the main human-facing overview; use `docs/` for deeper explanations such as rule configuration and code-fix safety.
- The repo has existing `.github/copilot-instructions.md`; if it conflicts with this file, follow `AGENTS.md`.
- The source root is nested (`Sharpen.Analyzer/Sharpen.Analyzer/`), so double-check paths before editing.
- There is no application entry point or `dotnet run` workflow here; use build/test/pack commands instead.
- Analyzer changes often need coordinated edits across source, tests, release notes, and docs.
- If you are changing packaging or publishing behavior, check the GitHub Actions workflows in `.github/workflows/` for the exact CI commands.
