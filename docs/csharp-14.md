# C# 14 rules

## SHARPEN065: Use field-backed property

C# 14 introduces **field-backed properties** via the `field` keyword. This rule suggests converting eligible manual backing-field properties.

### Safe-to-fix

```csharp
class C
{
    private int _x;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }
}
```

Becomes:

```csharp
class C
{
    public int X
    {
        get { return field; }
        set { field = value; }
    }
}
```

## SHARPEN066: Use null-conditional assignment

This rule suggests replacing a simple null-guarded assignment:

```csharp
if (x != null) x.Member = rhs;
```

with a null-conditional assignment:

```csharp
x?.Member = rhs;
```

### Safe-to-fix

- The `if` condition is a simple `x != null` (or `null != x`) check.
- The `if` body contains exactly one statement.
- That statement is a simple assignment to a member of the checked expression.

### Do not fix

- The `if` body contains multiple statements.
- The assignment receiver differs from the null-checked expression.

```csharp
if (x != null)
{
    y.Member = rhs;
    x.Other = 1;
}
```

## SHARPEN067: Use unbound generic type in nameof

This rule suggests replacing `nameof` on a constructed generic type:

```csharp
nameof(Dictionary<string, int>)
```

with the unbound generic form:

```csharp
nameof(Dictionary<,>)
```

### Safe-to-fix

- The `nameof(...)` argument is a constructed generic type.
- The unbound generic form is valid and still binds.

### Do not fix

- The `nameof(...)` argument is not a constructed generic type.

```csharp
nameof(C)
```

## SHARPEN068: Use lambda parameter modifiers without types

C# 14 allows lambda parameter modifiers without explicit parameter types.

This rule suggests removing redundant parameter types when they are only present to allow modifiers:

```csharp
Func<int, int> f = (ref int x) => x;
```

Becomes:

```csharp
Func<int, int> f = (ref x) => x;
```

### Safe-to-fix

- The lambda is target-typed to a known delegate type.
- Removing explicit parameter types does not change binding.

### Do not fix

- The lambda is not target-typed (e.g. `var f = (ref int x) => x;`).

## SHARPEN069: Remove redundant span conversion

C# 14 introduces additional implicit conversions to `Span<T>` / `ReadOnlySpan<T>`. This rule suggests removing redundant explicit conversions such as `AsSpan()` when they add no semantic value.

### Safe-to-fix

- The `AsSpan()` call is used as an argument to another invocation.
- Removing the conversion does not change overload resolution (the invoked symbol remains the same).

### Do not fix

- Removing the conversion would select a different overload.

```csharp
void M(int[] a) { }
void M(ReadOnlySpan<int> s) { }

M(a.AsSpan()); // do not fix: would change to M(int[])
```

## SHARPEN070: Use extension blocks

C# 14 introduces **extension blocks**:

```csharp
static class Extensions
{
    extension string
    {
        public static int A(this string s) => s.Length;
        public static int B(this string s) => s.GetHashCode();
    }
}
```

This rule suggests grouping multiple extension methods that share the same receiver type into an extension block.

### Safe-to-fix (conservative)

The analyzer reports when a `static` class contains **at least two** extension methods for the same receiver type.

### Do not fix automatically

A code fix may be offered only in very conservative cases (same file, non-`partial`, no preprocessor directives). The current implementation is intentionally limited.

## SHARPEN071: Consider partial constructors

C# 14 introduces **partial constructors**. This rule is informational and targets codebases that use source generation patterns.

### When it triggers

- A constructor body contains a call to a **partial method** with a common initialization name (e.g. `InitializeGenerated`, `OnConstructed`, `InitializeComponent`).

### Caveats

- No automatic fix is offered: adopting partial constructors typically requires generator support and project-wide conventions.

## SHARPEN072: Consider partial events

C# 14 introduces **partial events**. This rule is informational and targets codebases that use source generation patterns.

### When it triggers

- An event declaration has explicit `add`/`remove` accessors.
- Both accessors delegate to **partial methods**.

### Caveats

- No automatic fix is offered: adopting partial events typically requires generator support and project-wide conventions.
