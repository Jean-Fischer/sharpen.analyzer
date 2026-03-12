# C# 13 rules

## SHARPEN058 Prefer params collections

Suggest migrating non-public `params T[]` to collection-based `params` (e.g. `ReadOnlySpan<T>`) when the method body does not rely on array-only semantics.

**Code fix:** Yes (solution-wide, non-public only)

## SHARPEN059 Use from-end index in object initializers

Suggest using `^` indices in object/collection initializers when the pattern is provably equivalent.

**Code fix:** Yes

## SHARPEN060 Use `\e` escape sequence

Suggest replacing `\u001b` / `\x1b` with `\e` when unambiguous.

**Code fix:** Yes

## SHARPEN061 Use `System.Threading.Lock`

Suggest migrating dedicated private lock objects to `System.Threading.Lock` when the type is available and the field is only used in `lock` statements (no `Monitor.*` usage).

**Code fix:** Yes

## SHARPEN062 Partial properties/indexers refactoring

Suggest/refactor eligible members to C# 13 partial properties/indexers.

**Current implementation limitations:**

- Only auto-properties are supported.
- Indexers are currently excluded (Roslyn preview support in this repo does not allow auto-indexers without bodies).
- The refactoring is only offered inside `partial` types.

**Code fix:** No (refactoring-style code action)

## SHARPEN063 Suggest allows ref struct constraint

Guidance-only: suggest `allows ref struct` for eligible generic APIs.

**Important:** This is a library design decision and must be reviewed for ref-safety implications.

**Code fix:** No

## SHARPEN064 Suggest `OverloadResolutionPriorityAttribute`

Guidance-only: suggest `OverloadResolutionPriorityAttribute` for eligible overload sets.

**Important:** This is a library design decision and must be reviewed.

**Code fix:** No
