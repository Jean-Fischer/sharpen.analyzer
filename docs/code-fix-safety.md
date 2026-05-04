# Understanding Sharpen code-fix safety

Sharpen is conservative about automated changes.

Its goal is not only to find places where newer C# syntax or APIs could be used, but also to avoid offering fixes that may silently change behavior.

## Why a diagnostic might not have a fix

Some rules deliberately separate detection from automation:

- the analyzer can recognize a modernization opportunity
- the fix provider is only offered when Sharpen can prove the change is safe enough to apply automatically

As a result, you may sometimes see a diagnostic even though no code fix is offered.

## What Sharpen protects against

The safety checks are intentionally cautious around cases like:

- overload resolution changes
- evaluation order changes
- semantic differences hidden by similar syntax
- transformations that depend on surrounding context that cannot be proven safe

When Sharpen is unsure, it prefers to withhold the fix.

## What this means for package users

If you see a diagnostic without a fix:

1. treat it as a review prompt rather than an automatic refactoring
2. decide manually whether the newer pattern fits your code
3. use `.editorconfig` to reduce or disable the rule if it is too noisy for your project

## Guidance-only rules

Some rules are intentionally informational. They highlight design opportunities, but they do not provide an automated change because the decision depends on API design, library compatibility, or project-wide conventions.

Examples include some newer-language guidance rules in the C# 13 and C# 14 rule sets.

## More detail

Package users usually only need the behavior described above.

If you want the implementation details behind the safety pipeline, see [fix-provider-safety-checkers.md](fix-provider-safety-checkers.md).
