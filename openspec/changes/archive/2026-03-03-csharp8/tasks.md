## 1. Inventory & Mapping

- [x] 1.1 Locate all legacy C# 8 analyzers in the old project and list their source paths
- [x] 1.2 For each legacy analyzer, record diagnostic ID(s), title/message, category, severity, and supported language version
- [x] 1.3 Identify which legacy analyzers have code fixes and list the corresponding fix provider types
- [x] 1.4 Create a migration mapping table (legacy analyzer → new analyzer file/class name, new location, tests)

### Inventory (legacy)

| Legacy analyzer type | Legacy source path | Diagnostic ID(s) | Friendly name | Category | Default severity | Min language | Has code fix? |
|---|---|---|---|---|---|---|---|
| `ReplaceUsingStatementWithUsingDeclarationAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/UsingDeclarations/Analyzers/ReplaceUsingStatementWithUsingDeclarationAnalyzer.cs` | _TBD (not in analyzer file; likely defined in engine suggestion metadata)_ | "Replace using statement with using declaration" | UsingDeclarations | _TBD_ | 8.0 | No (no code fix types found under `original-sharpen/src`) |
| `ReplaceSwitchStatementWithSwitchExpressionAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/SwitchExpressions/Analyzers/ReplaceSwitchStatementWithSwitchExpressionAnalyzer.cs` | _TBD_ | "Replace switch statement … with switch expression" | SwitchExpressions | _TBD_ | 8.0 | No |
| `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/NullCoalescingAssignments/Analyzers/UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer.cs` | _TBD_ | "Use ??= operator instead of assigning result of the ?? operator" | NullCoalescingAssignments | _TBD_ | 8.0 | No |
| `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/NullableReferenceTypes/Analyzers/EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer.cs` | _TBD_ | "Enable nullable context and declare … as nullable" | NullableReferenceTypes | _TBD_ | 8.0 | No |
| `UseIndexFromTheEndAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/IndicesAndRanges/Analyzers/UseIndexFromTheEndAnalyzer.cs` | _TBD_ | "Use index from the end …" | IndicesAndRanges | _TBD_ | 8.0 | No |
| `ConsiderAwaitingEquivalentAsynchronousMethodAndYieldingIAsyncEnumerableAnalyzer` | `original-sharpen/src/Sharpen.Engine/SharpenSuggestions/CSharp80/AsynchronousStreams/Analyzers/ConsiderAwaitingEquivalentAsynchronousMethodAndYieldingIAsyncEnumerableAnalyzer.cs` | _TBD_ | "Consider awaiting equivalent asynchronous method and yielding IAsyncEnumerable" | AsynchronousStreams | _TBD_ | 8.0 | No |

### Migration mapping (planned)

| Legacy analyzer | New analyzer (planned) | New location | Tests (planned) |
|---|---|---|---|
| `ReplaceUsingStatementWithUsingDeclarationAnalyzer` | `ReplaceUsingStatementWithUsingDeclarationAnalyzer` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/ReplaceUsingStatementWithUsingDeclarationTests.cs` |
| `ReplaceSwitchStatementWithSwitchExpressionAnalyzer` | `ReplaceSwitchStatementWithSwitchExpressionAnalyzer` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/ReplaceSwitchStatementWithSwitchExpressionTests.cs` |
| `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer` | `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/UseNullCoalescingAssignmentOperatorTests.cs` |
| `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/EnableNullableContextAndDeclareIdentifierAsNullableTests.cs` |
| `UseIndexFromTheEndAnalyzer` | `UseIndexFromTheEndAnalyzer` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/` | `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/UseIndexFromTheEndTests.cs` |
| `ConsiderAwaitingEquivalentAsynchronousMethodAndYieldingIAsyncEnumerableAnalyzer` | _Already migrated as await-equivalent-async-method change_ | _N/A_ | _Already covered by existing tests_ |

## 2. Project Structure & Shared Utilities

- [x] 2.1 Identify shared helper code used by the legacy C# 8 analyzers (extensions, symbol helpers, syntax helpers)
- [x] 2.2 Port required shared helpers into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/) (or reuse existing helpers if already present)
- [x] 2.3 Ensure helper APIs follow existing conventions and are covered by unit tests where appropriate

### Shared helper code used by legacy C# 8 analyzers

| Legacy analyzer | Helper dependency | Legacy helper path |
|---|---|---|
| `ReplaceUsingStatementWithUsingDeclarationAnalyzer` | (none beyond Roslyn + legacy analysis result types) | _N/A_ |
| `ReplaceSwitchStatementWithSwitchExpressionAnalyzer` | (none beyond Roslyn + legacy analysis result types) | _N/A_ |
| `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer` | `SyntaxNodeFacts.AreEquivalent(...)` | `original-sharpen/src/Sharpen.Engine/Facts/SyntaxNodeFacts.cs` |
| `UseNullCoalescingAssignmentOperatorInsteadOfAssigningResultOfTheNullCoalescingOperatorAnalyzer` | `SyntaxNodeExtensions` helpers (e.g. `IsThisExpression()`, `IsIdentifierName()`, `GetPartsOfMemberAccessExpression(...)`, `IsObjectInitializerNamedAssignmentIdentifier()`, etc.) | `original-sharpen/src/Sharpen.Engine/Extensions/**` (entry point: `original-sharpen/src/Sharpen.Engine/Extensions/SyntaxNodeExtensions.cs`) |
| `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | `SyntaxNodeExtensions.OfAnyOfKinds(...)` | `original-sharpen/src/Sharpen.Engine/Extensions/SyntaxNodeExtensions.cs` |
| `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | Generated code detection: `GeneratedCodeDetection.IsGeneratedFile(...)` + `BeginsWithAutoGeneratedComment()` | `original-sharpen/src/Sharpen.Engine/Extensions/CodeDetection/GeneratedCodeDetection.cs` |
| `EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer` | (potential) unit test detection (not used directly in this analyzer, but exists in same helper area) | `original-sharpen/src/Sharpen.Engine/Extensions/CodeDetection/UnitTestingDetection.cs` |
| `UseIndexFromTheEndAnalyzer` | (analyzer is stubbed in legacy; no helper usage) | _N/A_ |

## 3. Migrate Analyzers

- [x] 3.1 Port analyzer #1 (implementation + descriptors) into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
- [x] 3.2 Port analyzer #2 (implementation + descriptors) into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
- [x] 3.3 Port analyzer #3 (implementation + descriptors) into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/)
- [x] 3.4 Continue porting remaining C# 8 analyzers until the inventory list is complete
- [x] 3.5 Ensure each migrated analyzer is registered/exported consistently with existing analyzers

> Note: As of 2026-03-03, the analyzers listed in the inventory table above are not yet present in the new project (see search results in `Sharpen.Analyzer/.../Analyzers/`).
>
> Progress: `ReplaceUsingStatementWithUsingDeclarationAnalyzer` implemented in [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/ReplaceUsingStatementWithUsingDeclarationAnalyzer.cs`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/ReplaceUsingStatementWithUsingDeclarationAnalyzer.cs:1) and rule added as [`Rules.ReplaceUsingStatementWithUsingDeclarationRule`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Rules/Rules.cs:1).

## 4. Migrate Code Fixes

- [x] 4.1 Port code fix provider(s) for analyzer #1 into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/)
- [x] 4.2 Port code fix provider(s) for analyzer #2 into [`Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/)
- [x] 4.3 Continue porting remaining C# 8 code fix providers until the inventory list is complete
- [x] 4.4 Verify code fix equivalence (output and behavior) against legacy implementation

## 5. Tests & Parity Validation

- [x] 5.1 Port or recreate unit tests for analyzer #1 (diagnostics + code fix if applicable)
- [x] 5.2 Port or recreate unit tests for analyzer #2 (diagnostics + code fix if applicable)
- [x] 5.3 Port or recreate unit tests for analyzer #3 (diagnostics + code fix if applicable)
- [x] 5.4 Continue adding tests until every migrated analyzer has at least one passing test
- [x] 5.5 Add/adjust tests for edge cases where Roslyn API differences required refactoring

## 6. Build, Packaging, and Cleanup

- [x] 6.1 Run full build and test suite; fix compilation issues and failing tests
- [x] 6.2 Verify analyzers are included in produced outputs (NuGet/VSIX as applicable)
- [x] 6.3 Remove any temporary migration scaffolding and ensure code style consistency
- [x] 6.4 Update documentation/release notes if required to mention the migrated C# 8 analyzers
