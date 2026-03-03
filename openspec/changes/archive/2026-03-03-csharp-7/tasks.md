## 1. Inventory upstream C# 7 feature set

- [x] 1.1 Locate the upstream C# 7 analyzers/code fixes in [`original-sharpen/`](original-sharpen/) and list the rules to migrate
- [x] 1.2 Identify any upstream shared helpers required by those rules (syntax helpers, resolvers, etc.)
- [x] 1.3 Identify upstream unit tests and sample snippets corresponding to the C# 7 rules

## 2. Port analyzers and code fixes

- [x] 2.1 Create/port analyzer classes into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/) for each upstream C# 7 rule
- [x] 2.2 Create/port code fix providers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/) for each rule that has an upstream fix
- [x] 2.3 Port minimal required shared helpers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/) (only when no local equivalent exists)
- [x] 2.4 Ensure diagnostic IDs, titles, categories, and severities match existing conventions (or document any necessary mapping)
- [x] 2.5 Wire analyzers into the package registration (supported diagnostics, exports) following existing patterns

## 3. Port tests

- [x] 3.1 Add analyzer tests for each migrated rule in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/)
- [x] 3.2 Add code fix tests for each migrated fix provider (where applicable)
- [x] 3.3 Ensure tests cover at least one positive and one negative case per rule

## 4. Port samples

- [x] 4.1 Create a `csharp7` folder in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/) mirroring the C# 6 sample organization
- [x] 4.2 Port upstream C# 7 sample snippets into the new `csharp7` folder
- [x] 4.3 Ensure sample code compiles and demonstrates each migrated rule

## 5. Validation and cleanup

- [x] 5.1 Build the solution and fix compilation issues introduced by the migration
- [x] 5.2 Run the full test suite and fix failing tests
- [x] 5.3 Verify analyzers trigger on the sample project and code fixes apply cleanly
- [x] 5.4 Update any documentation/readme references if the project lists supported C# versions or rules
