# Configuring Sharpen rules

Sharpen rules are configured through `.editorconfig`, the same way as other Roslyn analyzers.

This guide covers the most common package-user configuration tasks.

## Set a rule severity

Use the diagnostic ID to set the severity you want:

```ini
dotnet_diagnostic.SHARPEN004.severity = warning
```

Common values include:

- `error`
- `warning`
- `suggestion`
- `silent`
- `none`

## Disable a rule

To turn off a rule entirely:

```ini
dotnet_diagnostic.SHARPEN041.severity = none
```

## Promote informational rules

Many Sharpen rules are intentionally gentle by default. If your team wants stronger enforcement, raise them in `.editorconfig`:

```ini
dotnet_diagnostic.SHARPEN002.severity = warning
dotnet_diagnostic.SHARPEN044.severity = warning
dotnet_diagnostic.SHARPEN052.severity = warning
```

## Scope settings to a project or directory

Place `.editorconfig` at the repository root to apply rules broadly, or in a subdirectory to tailor behavior for a specific project.

Example:

```ini
root = true

[*.cs]
dotnet_diagnostic.SHARPEN004.severity = warning
dotnet_diagnostic.SHARPEN041.severity = suggestion
```

## Know when a rule can run

Sharpen groups rules by the C# language version they target. A rule will only apply when the project supports the relevant syntax or language feature.

The rule list in the main [README](..\Readme.md) is the quickest reference for supported rules and fix availability.

## Related guides

- [Getting started](getting-started.md)
- [Applying code fixes](applying-code-fixes.md)
- [Understanding code-fix safety](code-fix-safety.md)
