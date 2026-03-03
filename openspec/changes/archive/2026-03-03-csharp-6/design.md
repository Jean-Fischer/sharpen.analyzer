## Overview

This change adds C# 6-focused analyzers (and selective code fixes) to Sharpen.Analyzer.
The implementation should follow the existing project conventions:
- One analyzer per rule under `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Analyzers/`.
- One code fix provider per rule under `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/` when a fix is supported.
- One test class per analyzer/fix under `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/`.
- Sample code under `Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Sample/`.

The scope decision is:
- Implement analyzers for all 4 capabilities.
- Implement code fixes only for the subset that had fixes in the legacy Sharpen project.

## Capabilities and Rule Mapping

| Capability | Analyzer responsibility | Potential code fix |
|---|---|---|
| `use-expression-body-for-get-only-properties` | Detect get-only properties with a single `return <expr>;` body that can be expressed as `=> <expr>;` | Convert block-bodied get-only property to expression-bodied property (if supported) |
| `use-expression-body-for-get-only-indexers` | Detect get-only indexers with a single `return <expr>;` body that can be expressed as `=> <expr>;` | Convert block-bodied get-only indexer to expression-bodied indexer (if supported) |
| `use-nameof-expression-for-throwing-argument-exceptions` | Detect `throw new ArgumentXxxException("param")` where the string literal matches a parameter in scope | Replace string literal with `nameof(param)` (if supported) |
| `use-nameof-expression-in-dependency-property-declarations` | Detect dependency property registrations that pass a property name as a string literal | Replace string literal with `nameof(Property)` (if supported) |

## Analyzer Design

### General approach

- Use Roslyn syntax node actions to find candidate syntax nodes cheaply.
- Use semantic model checks only after fast syntactic filtering.
- Report diagnostics on the most relevant token:
  - Expression-bodied members: report on the property/indexer identifier.
  - `nameof` rules: report on the string literal token.

### Expression-bodied members

Target patterns:
- `PropertyDeclarationSyntax` with:
  - `AccessorList` containing exactly one `get` accessor
  - `get` accessor has a block body with exactly one statement: `return <expr>;`
  - no `set` accessor
  - no expression body already
- `IndexerDeclarationSyntax` with the same constraints.

Non-goals / exclusions:
- Do not suggest when the getter has multiple statements.
- Do not suggest when the getter uses `yield`.
- Do not suggest when there are attributes/trivia that would be lost (ensure trivia is preserved if a fix exists).

### `nameof` for argument exceptions

Target patterns:
- `ThrowStatementSyntax` where the expression is `ObjectCreationExpressionSyntax`.
- The created type is one of:
  - `System.ArgumentNullException`
  - `System.ArgumentException`
  - `System.ArgumentOutOfRangeException`
  - `System.ComponentModel.InvalidEnumArgumentException`
- One of the constructor arguments corresponds to the parameter-name argument (`paramName` or `argumentName`) and is a string literal.
- The string literal value matches a parameter name visible in the throw statement scope.

This mirrors the legacy Sharpen logic (fast checks first, semantic confirmation last).

### `nameof` in dependency property declarations

Target patterns:
- Dependency property registration calls (WPF):
  - `DependencyProperty.Register(...)`
  - `DependencyProperty.RegisterAttached(...)`
  - (and any other patterns found in legacy smoke tests)
- The first argument is a string literal representing the property name.
- A corresponding CLR property exists in the containing type (or can be inferred from the registration call).

Because this can be framework-specific, the spec will define the exact supported patterns.

## Code Fix Design (Selective)

Only implement code fixes for rules that had them in the legacy project.
If legacy did not provide a fix, the analyzer still reports diagnostics.

### Expression-bodied member fix

If supported:
- Replace the getter block body with an expression body:
  - `get { return expr; }` → `=> expr;`
- Remove the accessor list.
- Preserve trivia and formatting.

### `nameof` fix

If supported:
- Replace the string literal expression with `nameof(<identifier>)`.
- Preserve trivia.

## Testing Strategy

- Add unit tests for each analyzer:
  - Positive cases (diagnostic reported)
  - Negative cases (no diagnostic)
- If a code fix exists:
  - Verify the fixed code output.
  - Include edge cases (trivia, formatting, different constructor overloads).

## Samples

Add a C# 6 sample file (or extend an existing one) with one method per rule to validate:
- Diagnostics appear in the IDE.
- Code fixes appear only for the supported subset.

## Open Questions / To Confirm

- For each of the 4 rules, confirm whether the legacy project had an automatic fix.
  - If legacy had no fix infrastructure for these, we will implement analyzers only.
  - If legacy had fixes (e.g., via a VS extension refactoring), we will mirror that behavior with Roslyn code fixes.
