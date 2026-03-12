# Fix provider safety checkers

This project uses a **unified safety pipeline** to ensure we only suggest transformations that are safe.

The pipeline is executed by [`FixProviderSafetyRunner`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/FixProviderSafetyRunner.cs) and has two stages:

1. **Global stage**: [`FirstPassSafety`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafety.cs) gate (cross-cutting, conservative)
2. **Local stage**: per-fix-provider checker implementing [`IFixProviderSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/IFixProviderSafetyChecker.cs)

Short-circuit rules:

- If the global stage is unsafe, the local checker is not evaluated.
- If either stage is unsafe, we suppress both diagnostics and code actions.

## One-to-one mapping

Each fix provider should have a corresponding safety checker:

- Fix provider: applies the transformation (code action)
- Safety checker: validates the transformation is safe to offer

The mapping is defined in [`FixProviderSafetyMapping.cs`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/FixProviderSafetyMapping.cs).

## Mapping summary table

| Fix provider | Safety checker | Notes |
|---|---|---|
| [`UseCollectionExpressionCodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp12/UseCollectionExpressionCodeFixProvider.cs) | [`CollectionExpressionSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/CollectionExpressionSafetyChecker.cs) | Implemented |
| [`UseInterpolatedStringCodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp10/UseInterpolatedStringCodeFixProvider.cs) | [`StringInterpolationSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/StringInterpolationSafetyChecker.cs) | Implemented |
| [`PreferParamsCollectionsCodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/FixProvider/CSharp13/PreferParamsCollectionsCodeFixProvider.cs) | [`PreferParamsCollectionsSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/PreferParamsCollectionsSafetyChecker.cs) | Implemented |
| [`UseFromEndIndexInObjectInitializersCodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/FixProvider/CSharp13/UseFromEndIndexInObjectInitializersCodeFixProvider.cs) | [`UseFromEndIndexInObjectInitializersSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/UseFromEndIndexInObjectInitializersSafetyChecker.cs) | Implemented |
| [`UseEscapeSequenceECodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.FixProviders/FixProvider/CSharp13/UseEscapeSequenceECodeFixProvider.cs) | [`UseEscapeSequenceESafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/UseEscapeSequenceESafetyChecker.cs) | Implemented |
| (planned) `NullCheckFixProvider` | [`NullCheckSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/NullCheckSafetyChecker.cs) | Placeholder (no fix provider in repo yet) |
| (planned) `SwitchExpressionFixProvider` | [`SwitchExpressionSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/SwitchExpressionSafetyChecker.cs) | Placeholder (no fix provider in repo yet) |
| (planned) `LinqFixProvider` | [`LinqSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/LinqSafetyChecker.cs) | Placeholder (no fix provider in repo yet) |

## Integration flow

### Analyzer pipeline

1. Analyzer matches a pattern.
2. Analyzer calls `FixProviderSafetyRunner` (global stage).
3. If safe, analyzer reports a diagnostic.

> Note: analyzer-side local checker evaluation is currently not executed because checkers require a `Document` instance.
> The global stage still ensures diagnostics are suppressed when the global gate blocks.

Example (simplified):

```csharp
// Analyzer
var evaluation = FixProviderSafetyRunner.Evaluate(
    semanticModel: context.SemanticModel,
    fixProviderType: typeof(UseCollectionExpressionCodeFixProvider),
    node: matchedNode,
    diagnostic: null,
    cancellationToken: context.CancellationToken);

if (evaluation.Outcome != FixProviderSafetyOutcome.Safe)
    return;

context.ReportDiagnostic(Diagnostic.Create(rule, matchedNode.GetLocation()));
```

### Fix provider pipeline

1. Fix provider locates the node from the diagnostic.
2. Fix provider calls `FixProviderSafetyRunner` (global stage then local stage).
3. If safe, fix provider registers the code action.

Example (simplified):

```csharp
// CodeFixProvider
var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
if (semanticModel is null)
    return;

var evaluation = FixProviderSafetyRunner.Evaluate(
    semanticModel: semanticModel,
    fixProviderType: typeof(UseInterpolatedStringCodeFixProvider),
    node: node,
    diagnostic: diagnostic,
    cancellationToken: context.CancellationToken);

if (evaluation.Outcome != FixProviderSafetyOutcome.Safe)
    return;

context.RegisterCodeFix(action, diagnostic);
```

## Example: end-to-end (collection expressions)

- Analyzer: [`UseCollectionExpressionAnalyzer`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/CSharp12/UseCollectionExpressionAnalyzer.cs)
- Fix provider: [`UseCollectionExpressionCodeFixProvider`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/CSharp12/UseCollectionExpressionCodeFixProvider.cs)
- Safety checker: [`CollectionExpressionSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/CollectionExpressionSafetyChecker.cs)

Flow:

1. Analyzer finds an array creation that can be modernized.
2. Analyzer calls `FixProviderSafetyRunner` for `UseCollectionExpressionCodeFixProvider`.
3. If safe, analyzer reports `UseCollectionExpressionRule`.
4. Fix provider receives the diagnostic and calls `FixProviderSafetyRunner` again.
5. If safe, fix provider registers the code action.

## Adding a new fix provider + safety checker

1. Create a new checker implementing [`IFixProviderSafetyChecker`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/IFixProviderSafetyChecker.cs).
2. Add a mapping entry in [`FixProviderSafetyMapping.cs`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FixProviderSafety/FixProviderSafetyMapping.cs).
3. Gate the analyzer before `ReportDiagnostic` by calling `FixProviderSafetyRunner`.
4. Gate the fix provider before `RegisterCodeFix` by calling `FixProviderSafetyRunner`.
5. Do not call [`FirstPassSafetyRunner`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafetyRunner.cs) directly.
6. Add tests for the checker and at least one integration test that exercises the global gate.

## Deprecation note: `FirstPassSafetyRunner`

[`FirstPassSafetyRunner`](../Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Safety/FirstPassSafetyRunner.cs) is deprecated and should not be used by fix providers.

- Existing call sites have been migrated to `FixProviderSafetyRunner`.
- New code should always use `FixProviderSafetyRunner` so the global + local stages remain consistent.

## Notes

- Safety checkers should be conservative: return unsafe if unsure.
- Analyzer-side gating prevents noisy diagnostics.
- Fix-provider-side gating prevents offering unsafe code actions even if a diagnostic exists.
