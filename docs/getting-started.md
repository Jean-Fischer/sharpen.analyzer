# Getting started with sharpen.analyzer

This tutorial helps a first-time package user install Sharpen, confirm it is active, and apply a first fix.

By the end, you will have Sharpen installed in a project and know where to look for diagnostics and fixes.

## 1. Choose the package you want

Sharpen is published as two packages:

| Package | Includes | Use it when |
|---|---|---|
| `Sharpen.Analyzer` | Diagnostics only | You want analyzer output without code fixes. |
| `Sharpen.Analyzer.FixProviders` | Diagnostics and code fixes | You want the full experience in the IDE and through bulk fixes. |

For most users, `Sharpen.Analyzer.FixProviders` is the better default.

## 2. Install the package

Install one package into the project that should receive the diagnostics.

### Diagnostics only

```bash
dotnet add package Sharpen.Analyzer
```

### Diagnostics and code fixes

```bash
dotnet add package Sharpen.Analyzer.FixProviders
```

If you install `Sharpen.Analyzer.FixProviders`, you do not need to install `Sharpen.Analyzer` separately.

## 3. Build the project

Run a normal build:

```bash
dotnet build
```

If Sharpen finds eligible code, its diagnostics appear in the build output just like other Roslyn analyzer diagnostics.

## 4. Review diagnostics in the IDE

Open the project in a Roslyn-based editor such as Visual Studio or Rider.

Sharpen diagnostics appear in the editor, the Error List, and other analyzer surfaces provided by the IDE.

## 5. Apply your first fix when one is available

If you installed `Sharpen.Analyzer.FixProviders`, many rules offer Quick Actions.

Open one diagnostic, trigger Quick Actions, and apply the suggested change if the rule offers one.

For more detailed workflows, see:

- [Applying code fixes](applying-code-fixes.md)
- [Understanding code-fix safety](code-fix-safety.md)

## 6. Adjust rule severity if needed

Sharpen follows standard Roslyn analyzer configuration through `.editorconfig`.

Example:

```ini
dotnet_diagnostic.SHARPEN004.severity = warning
dotnet_diagnostic.SHARPEN041.severity = none
```

For more examples, see [Configuring rules](configuring-rules.md).

## You are ready to continue

- Start with the main [README](..\Readme.md)
- Tune rule severity with [Configuring rules](configuring-rules.md)
- Apply changes at scale with [Applying code fixes](applying-code-fixes.md)
