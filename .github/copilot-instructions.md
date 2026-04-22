# Copilot instructions for `sharpen.analyzer`

## Build, test, and lint

- Restore: `dotnet restore Sharpen.Analyzer\Sharpen.Analyzer.sln`
- Build: `dotnet build Sharpen.Analyzer\Sharpen.Analyzer.sln --configuration Release --no-restore`
- Test: `dotnet test Sharpen.Analyzer\Sharpen.Analyzer.sln --configuration Release --no-build --verbosity normal`
- Single test: `dotnet test Sharpen.Analyzer\Sharpen.Analyzer.sln --configuration Release --no-build --filter FullyQualifiedName~UseVarKeywordCodeFixTests`
- Bulk analyzer fixes: `dotnet format analyzers "Sharpen.Analyzer\Sharpen.Analyzer\Sharpen.Analyzer.Sample\Sharpen.Analyzer.Sample.csproj" --verbosity detailed --severity info`

## High-level architecture

- The solution is `Sharpen.Analyzer\Sharpen.Analyzer.sln` and contains four projects: `Sharpen.Analyzer`, `Sharpen.Analyzer.FixProviders`, `Sharpen.Analyzer.Tests`, and `Sharpen.Analyzer.Sample`.
- `Sharpen.Analyzer` is the analyzer assembly. It targets `netstandard2.0`, stays free of `Microsoft.CodeAnalysis.Workspaces`, and packs analyzer assemblies plus `AnalyzerReleases.Shipped.md` / `AnalyzerReleases.Unshipped.md`.
- `Sharpen.Analyzer.FixProviders` is the code-fix assembly. It targets `netstandard2.0`, references the analyzer project, can use Workspaces APIs, and packs its DLL under `analyzers/dotnet/cs`.
- Analyzer rules live in `Analyzers/CSharp*/` and centralize descriptors in `Rules/*.cs`.
- Code fixes live in `Sharpen.Analyzer.FixProviders/FixProvider/CSharp*/`.
- Safety logic is split out under `Safety/`: `FirstPassSafety*` orchestrates short-circuit safety checks, while `Safety/FixProviderSafety/*` maps each fix provider to a dedicated checker.
- `Sharpen.Analyzer.Tests` uses xUnit plus Roslyn testing helpers; the sample project demonstrates the package behavior.

## Key conventions

- Keep analyzer, fix provider, safety checker, and tests in sync for the same rule. Rule changes usually require edits in all four surfaces.
- Use the existing naming pattern: analyzer/fix provider/test class names mirror the rule name and language-version folder.
- `Rules/GeneralRules.cs` is the main source of diagnostic descriptors; update the README rule table when adding or changing supported rules.
- Safety checks are intentionally conservative. Prefer syntax checks first and only use semantic checks when needed.
- The analyzer assembly must not gain a Workspaces reference; put code-fix-only logic in `Sharpen.Analyzer.FixProviders`.
- Fix-provider safety mappings are resolved by type name at runtime to avoid a compile-time dependency from the analyzer assembly to the fix-provider assembly.
- Tests that touch global static state must remain non-parallel; `Sharpen.Analyzer.Tests/AssemblyInfo.cs` disables parallelization for that reason.
- Follow the existing xUnit pattern in tests: inline source strings, `Verifier` aliases, and separate analyzer vs. code-fix test files.
