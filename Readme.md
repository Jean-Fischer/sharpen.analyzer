# sharpen.analyzer

Roslyn analyzers + code fixes to help modernize C# codebases.

## Install

Two packages are published:

- [`Sharpen.Analyzer`](Readme.md:1): diagnostics only (analyzers)
- [`Sharpen.Analyzer.FixProviders`](Readme.md:1): diagnostics + IDE code fixes (depends on `Sharpen.Analyzer`)

Install **one** of them:

### Option A: analyzers only

```bash
dotnet add package Sharpen.Analyzer
```

### Option B: analyzers + code fixes (recommended)

```bash
dotnet add package Sharpen.Analyzer.FixProviders
```

`Sharpen.Analyzer.FixProviders` brings `Sharpen.Analyzer` automatically, so you don’t need to install both.

## Use

Once the package is referenced, diagnostics are produced by Roslyn-based IDEs/editors and during `dotnet build` / `dotnet test`.

### Safety gate (code fix suppression)

Some code fixes are protected by a conservative “first-pass safety gate”. When a match is found but the transformation is deemed unsafe, the analyzer may still report the diagnostic, but the code fix will not be offered.

### Enable / configure rules

Rules can be configured using `.editorconfig` (severity, enable/disable, etc.).

Example:

```ini
# .editorconfig

# SHARPEN004: Await Task.Delay instead of calling Thread.Sleep
# (example severity; adjust to your needs)
dotnet_diagnostic.SHARPEN004.severity = warning
```

## Rules

See the **Supported rules/features** section below (source of truth).

## Supported rules/features

Rules are grouped by the C# language version they target (when applicable).

### C# 3

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN002 | Use var keyword in variable declaration with object creation | Prefer `var` when the type is obvious from the right-hand side object creation. | Yes |

### C# 5

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN003 | Use async equivalent | In an `async` caller, replace a synchronous invocation with an equivalent `*Async` method (and add `await` when needed). | Yes |
| SHARPEN004 | Await Task.Delay instead of calling Thread.Sleep | Replace `Thread.Sleep(...)` with `await Task.Delay(...)` when the containing member can be made `async`. | Yes |
| SHARPEN005 | Await Task instead of calling Task.Result | Replace `.Result` with `await` when safe. | Yes |
| SHARPEN006 | Await Task instead of calling Task.Wait | Replace `.Wait()` with `await` when safe. | Yes |
| SHARPEN007 | Await Task.WhenAny instead of calling Task.WaitAny | Replace `Task.WaitAny(...)` with `await Task.WhenAny(...)` when safe. | Yes |
| SHARPEN008 | Await Task.WhenAll instead of calling Task.WaitAll | Replace `Task.WaitAll(...)` with `await Task.WhenAll(...)` when safe. | Yes |
| SHARPEN009 | Consider awaiting equivalent asynchronous method and making the caller asynchronous | Suggest using an equivalent `*Async` method and making the caller `async` when possible. | Yes |

### C# 6

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN010 | Use expression-bodied member for get-only property | Convert a get-only property with a single `return` into an expression-bodied property. | Yes |
| SHARPEN011 | Use expression-bodied member for get-only indexer | Convert a get-only indexer with a single `return` into an expression-bodied indexer. | Yes |
| SHARPEN012 | Use nameof expression for parameter name | Replace string-literal parameter names in thrown argument exceptions with `nameof(...)`. | Yes |
| SHARPEN013 | Use nameof expression for dependency property name | Replace string-literal dependency property names in `DependencyProperty.Register*` calls with `nameof(...)`. | Yes |

### C# 7

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN014 | Use expression-bodied member for get accessor in property | Convert a `get { return ...; }` accessor into `get => ...;` when possible. | Yes |
| SHARPEN015 | Use expression-bodied member for constructor | Convert a simple constructor body into an expression-bodied constructor. | Yes |
| SHARPEN016 | Use expression-bodied member for destructor | Convert a simple destructor body into an expression-bodied destructor. | Yes |
| SHARPEN017 | Use expression-bodied member for local function | Convert a simple local function body into an expression-bodied local function. | Yes |
| SHARPEN018 | Use out variables in method invocations | Use inline `out var` declarations in method calls. | Yes |
| SHARPEN019 | Use out variables in object creations | Use inline `out var` declarations in object creation expressions. | Yes |
| SHARPEN020 | Discard out variables in method invocations | Replace unused `out` variables with discards (`out _`). | Yes |
| SHARPEN021 | Discard out variables in object creations | Replace unused `out` variables with discards (`out _`). | Yes |

### C# 7.1

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN022 | Use default expression in return statements | Prefer `default` over `default(T)` when the type can be inferred. | Yes |
| SHARPEN023 | Use default expression in optional method parameters | Prefer `default` over `default(T)` in optional method parameters when the type can be inferred. | Yes |
| SHARPEN024 | Use default expression in optional constructor parameters | Prefer `default` over `default(T)` in optional constructor parameters when the type can be inferred. | Yes |

### C# 8

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN025 | Replace using statement with using declaration | Convert `using (...) { ... }` to a C# 8 using declaration when safe. | Yes |
| SHARPEN026 | Replace switch statement with switch expression | Convert a switch statement to a switch expression when safe. | Yes |
| SHARPEN027 | Consider replacing switch statement with switch expression | Suggest converting a switch statement to a switch expression (conservative/heuristic). | Yes |
| SHARPEN028 | Replace switch statement with switch expression | Convert a switch statement containing only assignments to a switch expression. | Yes |
| SHARPEN029 | Consider replacing switch statement with switch expression | Suggest converting a switch statement containing only returns to a switch expression (conservative/heuristic). | Yes |
| SHARPEN030 | Replace switch statement with switch expression | Convert a switch statement containing only returns to a switch expression. | Yes |
| SHARPEN031 | Use ??= operator instead of assigning result of the ?? operator | Replace `x = x ?? y` with `x ??= y` when safe. | Yes |
| SHARPEN032 | Consider using ??= operator instead of assigning result of the ?? operator | Suggest `??=` when conversion might change semantics due to side effects. | Yes |
| SHARPEN033 | Enable nullable context and declare identifier as nullable | Suggest enabling nullable context and marking identifiers as nullable where appropriate. | Yes |
| SHARPEN034 | Use index from the end | Suggest using `^` (index-from-end) syntax where applicable. | No |

### C# 9

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN035 | Use init-only setter | Convert `get; private set;` auto-properties to `init;` when safe. | Yes |
| SHARPEN036 | Use record type | Convert eligible `sealed class` data containers to `record`. | Yes |
| SHARPEN037 | Use top-level statements | Convert a simple `Program.Main` entry point to top-level statements. | Yes |
| SHARPEN038 | Use C# 9 pattern matching | Rewrite eligible boolean expressions using `is not`, relational patterns, and `and` patterns. | Yes |
| SHARPEN039 | Use target-typed new | Replace `new T(...)` with `new(...)` when the target type is known and safe. | Yes |

### C# 10

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN040 | Use file-scoped namespace | Convert `namespace X { ... }` to `namespace X;` when safe. | Yes |
| SHARPEN041 | Use global using directive | Suggest converting repeated `using` directives to `global using` (per-document fix; use “Fix all” to apply broadly). | Yes |
| SHARPEN042 | Use record struct | Convert eligible `struct` value objects to `record struct`. | Yes |
| SHARPEN043 | Use extended property pattern | Rewrite eligible expressions using C# 10 extended property patterns. | Yes |
| SHARPEN044 | Use interpolated string | Replace `string.Format(...)` / concatenation with interpolated strings when safe. | Yes |
| SHARPEN045 | Use const interpolated string | Replace constant concatenation / `string.Format` with const interpolated strings when safe. | Yes |

### C# 11

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN046 | Use raw string literal | Suggest raw string literals for multi-line or heavily-escaped strings. | Yes |
| SHARPEN047 | Use required member | Add `required` to eligible properties. | Yes |
| SHARPEN048 | Use generic math constraints | Suggest adding generic math constraints (e.g., `where T : INumber<T>`) when numeric operators are used on unconstrained type parameters. | No |
| SHARPEN049 | Use list pattern | Suggest list patterns for common span/array length + indexing patterns. | Yes (limited) |
| SHARPEN050 | Use UTF-8 string literal | Suggest replacing UTF-8 byte data with `"..."u8` when type-compatible. | Yes |

### C# 12

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN051 | Use primary constructor | Convert assignment-only constructors to primary constructors when safe. | Yes (experimental) |
| SHARPEN052 | Use collection expression | Convert eligible array/collection initializers to C# 12 collection expressions (`[...]`). | Yes |
| SHARPEN053 | Use default lambda parameters | Use default values in explicitly-typed lambda parameter lists when applicable. | Yes |
| SHARPEN054 | Use InlineArray | Convert fixed-size buffer-like structs to `[InlineArray(N)]` when safe. | Yes |

### C# 13

| Rule ID | Title | Description | Code fix |
|---|---|---|---|
| SHARPEN058 | Prefer params collections | Suggest migrating non-public `params T[]` to collection-based `params` when safe. | Yes |
| SHARPEN059 | Use from-end index in object initializers | Suggest using `^` indices in object/collection initializers when provably equivalent. | Yes |
| SHARPEN060 | Use \e escape sequence | Suggest replacing `\u001b` / `\x1b` with `\e` when unambiguous. | Yes |
| SHARPEN061 | Use System.Threading.Lock | Suggest migrating dedicated private lock objects to `System.Threading.Lock` when available and safe. | Yes |
| SHARPEN062 | Partial properties/indexers refactoring | Suggest/refactor eligible members to C# 13 partial properties/indexers when safe. | No (refactoring) |
| SHARPEN063 | Suggest allows ref struct constraint | Guidance-only: suggest `allows ref struct` for eligible generic APIs. | No |
| SHARPEN064 | Suggest OverloadResolutionPriorityAttribute | Guidance-only: suggest `OverloadResolutionPriorityAttribute` for eligible overload sets. | No |

## Development

Open [Sharpen.Analyzer/Sharpen.Analyzer.sln](Sharpen.Analyzer/Sharpen.Analyzer.sln:1) and run the test project (`Sharpen.Analyzer.Tests`).

## Contributing

See [todo.md](todo.md:1) for the current backlog and notes.
