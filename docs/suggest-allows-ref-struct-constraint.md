# SHARPEN063 Suggest allows ref struct constraint

C# 13 introduces the `allows ref struct` anti-constraint.

This rule is **guidance-only**: it highlights generic APIs that may benefit from accepting `ref struct` type arguments (e.g. span-like types).

## Why

Some generic APIs are designed to work with stack-only types (like `Span<T>` / `ReadOnlySpan<T>`), but without `allows ref struct` they cannot accept `ref struct` type arguments.

## What the analyzer looks for

The analyzer uses a conservative heuristic and currently reports when:

- A generic method/type has **no existing constraints**, and
- It uses a type parameter in a span-like position (e.g. `Span<T>` / `ReadOnlySpan<T>`), or in a by-ref position.

## Important: requires review

Adding `allows ref struct` is a **library design decision** and must be reviewed for ref-safety implications.

Sharpen does not provide an automatic code fix for this rule.
