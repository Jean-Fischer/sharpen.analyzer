# OpenSpec Change Proposal: C# 9 rules

- Change key: `csharp-9`
- Status: Proposal
- Scope: Add analyzers/code fixes and tests for selected C# 9 language features.
- Non-goal: No implementation in this change proposal.

## Motivation
Sharpen’s mission is to help developers modernize and learn newer C# features through actionable diagnostics and safe code fixes. C# 9 introduced several high-impact features that are frequently encountered in modern codebases:

- Init-only setters
- Record types
- Top-level statements
- Pattern matching enhancements
- Target-typed `new`

This change proposes a set of rules that:

1. Detect pre-C# 9 patterns that can be modernized.
2. Offer code fixes that are safe, semantics-preserving, and style-consistent.
3. Provide an extensive test matrix to ensure correctness across edge cases.

## Guiding principles
- Prefer transformations that are **mechanical** and **semantics-preserving**.
- Avoid fixes that require non-local reasoning (e.g., cross-project API changes) unless explicitly constrained.
- Respect user code style where possible (formatting, trivia, naming).
- Ensure rules are **language-version aware** (only offer fixes when C# 9 is enabled).

## Proposed rules overview
This proposal defines five rule families. Each family may map to one or more diagnostics.

| Feature | Rule family | Primary intent | Fix type |
|---|---|---|---|
| Init-only setters | `UseInitOnlySetter` | Prefer `init` for immutable initialization | Local refactor |
| Record types | `UseRecordType` | Prefer `record` for data-centric types | Type refactor |
| Top-level statements | `UseTopLevelStatements` | Prefer top-level program for simple entry points | File refactor |
| Pattern matching enhancements | `UseCSharp9PatternMatching` | Prefer C# 9 patterns (`or`, `and`, `not`, relational) | Expression refactor |
| Target-typed `new` | `UseTargetTypedNew` | Prefer `new()` when type is obvious | Expression refactor |

## Rule family: Init-only setters (`UseInitOnlySetter`)

### Problem statement
Properties that are only assigned during object initialization are commonly implemented with `private set;` or `set;` plus conventions. C# 9 provides `init;` to express intent and enforce immutability after initialization.

### Detection criteria
Flag an auto-property (or property with trivial setter) that meets all of the following:

1. Property is declared in a `class`/`struct` (including `record class`/`record struct` if present).
2. Property has a `set` accessor (auto or trivial body) and is not already `init`.
3. Setter accessibility is `private` or `protected private` (configurable), OR analysis proves all assignments occur only in:
   - object initializer expressions (`new T { P = ... }`), and/or
   - constructors of the containing type.
4. Property is not assigned anywhere else (including within methods, property setters, event handlers, lambdas, local functions).
5. Property is not part of an interface implementation requiring `set`.
6. Property is not `abstract`.

Notes:
- Start with a conservative version: only `private set;` auto-properties.
- Expand later to dataflow-based detection.

### Code fix intent
Replace `set;` with `init;` while preserving:
- attributes on accessors
- accessor modifiers
- trivia/formatting

### Examples

**Before**
```csharp
public class Person
{
    public string Name { get; private set; }

    public Person(string name)
    {
        Name = name;
    }
}
```

**After**
```csharp
public class Person
{
    public string Name { get; init; }

    public Person(string name)
    {
        Name = name;
    }
}
```

### Non-examples / should not trigger
- Property assigned in a method after construction.
- Property implementing an interface with `set`.
- Property with non-trivial setter logic.

### Test matrix (init-only)
- Auto-property `private set;` → `init;`
- Auto-property `protected set;` (should not trigger by default)
- Auto-property `private set;` with attributes on setter
- Property with expression-bodied getter and auto setter
- Property assigned in constructor only (positive)
- Property assigned in constructor + method (negative)
- Property assigned in object initializer only (positive if dataflow enabled)
- Property assigned in lambda/local function (negative)
- Interface implementation (negative)
- `struct` vs `class`
- `record` types (if present)
- Nullable reference types enabled/disabled

## Rule family: Record types (`UseRecordType`)

### Problem statement
Many types are “data carriers” with value-based equality, deconstruction, and immutability patterns. C# 9 `record` provides concise syntax and correct value semantics.

### Detection criteria
Flag a `class` that appears to be a data-centric type and meets conservative criteria:

1. `class` is `sealed` OR has no virtual members and no derived types in solution (optional, likely out-of-scope initially).
2. Contains only:
   - auto-properties (get-only or get+private set/init), and/or
   - a constructor assigning those properties, and/or
   - overrides of `Equals(object)`, `GetHashCode()`, and `ToString()` that follow typical patterns.
3. No mutable fields (or only `readonly` fields).
4. No event declarations.
5. No explicit finalizer.
6. No unsafe code.

Given complexity, this rule may be split:
- `UseRecordTypeForSealedDataClass` (very conservative)
- `UseRecordTypeForEquatableDataClass` (requires more analysis)

### Code fix intent
Convert:
- `sealed class` → `record` (or `record class` depending on style)
- Constructor + properties → primary constructor when safe

Preserve:
- accessibility modifiers
- attributes
- XML docs
- base type / interfaces (records can inherit from `object` or another record; class-to-record with base class is tricky)

### Examples

**Before**
```csharp
public sealed class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

**After**
```csharp
public record Point(int X, int Y);
```

### Non-examples / should not trigger
- Class with mutable state or methods with behavior.
- Class inheriting from a non-record base class.
- Class with custom equality semantics not matching record behavior.

### Test matrix (records)
- `sealed class` with two get-only auto-properties + ctor → `record` primary ctor
- `sealed class` with init-only properties + ctor → `record` (keep properties or primary ctor)
- Class with method body (negative)
- Class with field mutation (negative)
- Class with base class (negative)
- Class implementing interfaces (positive if safe)
- Existing `IEquatable<T>` implementation (evaluate)
- Attributes on class/properties/ctor parameters
- XML docs preservation
- `#nullable enable` context

## Rule family: Top-level statements (`UseTopLevelStatements`)

### Problem statement
C# 9 allows omitting the `Program` class and `Main` method for simple applications, improving readability.

### Detection criteria
Flag a file containing a canonical entry point pattern:

1. A `class Program` (name configurable) with a single `static` `Main` method.
2. `Main` body contains only top-level-eligible statements (no local type declarations that would change semantics when moved).
3. No other members in `Program` class.
4. No other types in the file that depend on `Program` being a type (e.g., `typeof(Program)` usage).
5. No explicit `namespace` block that would be lost (C# 10 introduces file-scoped namespaces; for C# 9, keep namespace if present by leaving file as-is or avoid fix).

Given C# 9 limitations, the fix should be conservative:
- Only apply when file has no namespace declaration.
- Only apply when `Program` is in global namespace.

### Code fix intent
Replace:
- `class Program { static void Main(string[] args) { ... } }`
with:
- top-level statements `...`

Preserve:
- `using` directives
- comments/trivia
- `args` usage: if `args` referenced, keep `string[] args` implicit variable? (In top-level statements, `args` is available as `string[] args` implicitly.)

### Examples

**Before**
```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello");
    }
}
```

**After**
```csharp
using System;

Console.WriteLine("Hello");
```

### Non-examples / should not trigger
- Multiple types in file.
- `Main` is async returning `Task` (still possible, but ensure semantics).
- `Main` has unsafe code, `ref` locals, or local functions that capture `args` in tricky ways.

### Test matrix (top-level)
- `static void Main(string[] args)` simple body → top-level
- `static int Main()` returning int (ensure return mapping; top-level supports `return`)
- `static async Task Main()` (optional)
- `Main` with `args` usage
- File with namespace (negative)
- File with additional members (negative)
- File with `typeof(Program)` usage (negative)
- Preprocessor directives around `Main` (negative or ensure preserved)

## Rule family: Pattern matching enhancements (`UseCSharp9PatternMatching`)

### Problem statement
C# 9 adds:
- relational patterns (`x is > 0`)
- logical patterns (`and`, `or`, `not`)
- parenthesized patterns

These can simplify complex boolean expressions.

### Detection criteria
Flag boolean expressions that can be rewritten into `is` patterns without changing semantics:

1. Chains of comparisons against the same expression:
   - `x < a || x > b` → `x is < a or > b`
   - `x >= a && x <= b` → `x is >= a and <= b`
2. Negated `is` checks:
   - `!(x is T)` → `x is not T`
3. `x == null` / `x != null` patterns:
   - `x != null` → `x is not null`

Conservative constraints:
- Only when the compared expression is side-effect free (identifier, member access, `this`, `base`, simple invocation excluded).
- Preserve short-circuit semantics (patterns evaluate the input once; boolean chains may evaluate multiple times if expression repeated; require syntactic identity and side-effect free).

### Code fix intent
Rewrite to pattern matching forms:
- `expr != null` → `expr is not null`
- `!(expr is T)` → `expr is not T`
- Range checks to relational patterns with `and`/`or`

### Examples

**Before**
```csharp
if (x != null)
{
}
```

**After**
```csharp
if (x is not null)
{
}
```

**Before**
```csharp
if (value >= 0 && value <= 10)
{
}
```

**After**
```csharp
if (value is >= 0 and <= 10)
{
}
```

### Test matrix (pattern matching)
- `x != null` → `x is not null`
- `x == null` → `x is null` (optional)
- `!(x is null)` → `x is not null`
- `!(x is T)` → `x is not T`
- `x is T || x is U` → `x is T or U` (optional)
- `x is T && x is not U` → `x is T and not U` (optional)
- Range checks with constants and variables
- Expression with side effects (negative): `Get() != null`
- Expression repeated but not identical (negative)
- Parentheses precedence correctness

## Rule family: Target-typed `new` (`UseTargetTypedNew`)

### Problem statement
C# 9 allows omitting the type in `new` expressions when the target type is known, improving readability.

### Detection criteria
Flag `new T(...)` or `new T { ... }` when the type `T` is inferable from context:

1. Variable declaration with explicit type:
   - `List<int> x = new List<int>();` → `List<int> x = new();`
2. Assignment where LHS type is known and unambiguous:
   - `x = new List<int>();` where `x` is `List<int>`.
3. Return statement where return type is known and unambiguous:
   - `return new List<int>();` in method returning `List<int>`.
4. Argument position where parameter type is known and unambiguous:
   - `M(new List<int>());` where `M(List<int> p)`.

Conservative constraints:
- Do not apply when `T` is an anonymous type.
- Do not apply when `new` uses a type that differs from target via implicit conversion (e.g., `IEnumerable<int> x = new List<int>();` is still valid for `new()` but may change readability; decide policy).
- Do not apply when overload resolution could change (argument position can be tricky).

### Code fix intent
Replace `new T(...)` with `new(...)` or `new()` depending on syntax.

Preserve:
- object/collection initializer
- constructor arguments
- trivia

### Examples

**Before**
```csharp
List<int> xs = new List<int>();
```

**After**
```csharp
List<int> xs = new();
```

### Test matrix (target-typed new)
- Local variable explicit type + `new T()` → `new()`
- Field/property initializer
- `var x = new T()` (negative)
- Assignment to typed variable
- Return statement
- Argument position with single overload (positive)
- Argument position with overload ambiguity (negative)
- Object initializer `new T { P = 1 }` → `new() { P = 1 }`
- Collection initializer
- Generic types, nested generics
- `new int[0]` (negative; arrays not target-typed `new` in same way)

## Cross-cutting requirements

### Language version gating
All diagnostics and fixes must only be offered when the compilation language version is C# 9 or higher.

### Diagnostic metadata (proposed)
For each diagnostic:
- Category: `Modernization`
- Severity: `Info` (default)
- Help link: to be added later

### Code fix safety
- Fixes must preserve semantics.
- Fixes must preserve formatting and comments.
- Fixes must not introduce compilation errors.

## Test strategy (global)
- Unit tests per rule family using existing test infrastructure.
- Each rule family should have:
  - Positive tests (diagnostic + fix)
  - Negative tests (no diagnostic)
  - Edge tests (trivia, directives, generics, nullable)

## Deliverables (proposal only)
- `openspec/changes/csharp-9/proposal.md` (this document)
- Follow-up artifacts (design/specs/tasks) to be created after proposal approval.
