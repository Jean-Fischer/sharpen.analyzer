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
