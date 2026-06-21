# Applying Sharpen code fixes

Sharpen can modernize code in two main ways:

1. through Quick Actions in the IDE
2. through bulk fixes with `dotnet format analyzers`

This guide shows when to use each one.

## Before you start

Install `Sharpen.Analyzer.FixProviders` if you want code fixes. The `Sharpen.Analyzer` package alone reports diagnostics but does not provide IDE fixes.

## Apply fixes in the IDE

In Visual Studio, Rider, or another Roslyn-based editor:

1. Place the cursor on the diagnostic.
2. Open Quick Actions.
3. Apply the suggested Sharpen fix.

This works well when you want to review each change individually.

## Use Fix All for repeated patterns

For many IDE-supported fixes, "Fix all in document", "Fix all in project", or similar commands can apply the same transformation across a larger scope.

Use this when you want reviewable, incremental modernization inside the editor.

## Apply fixes from the command line

To apply supported analyzer fixes without using the IDE:

```bash
dotnet format analyzers "path\to\YourProject.csproj" --severity info
```

`--severity info` is important because many Sharpen rules are reported at `info` severity by default.

## When a fix is not offered

Some Sharpen rules are diagnostics only, and some rules intentionally suppress their code fix when the safety checks cannot prove the transformation is safe.

That means you may see:

- a diagnostic with an IDE fix
- a diagnostic without an IDE fix
- a rule listed as guidance-only with no automated change

For the reasoning behind this behavior, see [Understanding code-fix safety](code-fix-safety.md).

## Related guides

- [Getting started](getting-started.md)
- [Configuring rules](configuring-rules.md)
- [Understanding code-fix safety](code-fix-safety.md)
