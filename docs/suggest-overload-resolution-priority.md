# SHARPEN064 Suggest OverloadResolutionPriorityAttribute

C# 13 introduces `OverloadResolutionPriorityAttribute` to help guide overload selection.

This rule is **guidance-only**: it highlights overload sets that may benefit from explicitly prioritizing newer/more specific overloads.

## Why

Overload sets that include a broad "catch-all" overload (for example `params object[]`) can unintentionally capture calls that were meant for a more specific overload.

`OverloadResolutionPriorityAttribute` can be used by library authors to steer overload resolution.

## What the analyzer looks for

The analyzer uses a conservative heuristic and currently reports when a type contains an overload set where:

- There are multiple overloads with the same name, and
- At least one overload is a catch-all `params object[]` (or span-like equivalent).

## Important: requires review

Adding priorities is a **library design decision** and must be reviewed.

Sharpen does not provide an automatic code fix for this rule.
