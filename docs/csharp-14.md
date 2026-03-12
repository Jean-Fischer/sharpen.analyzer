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

### Do not fix

- The backing field is referenced outside the property accessors.
- The backing field name is used in `nameof(...)` or in attribute arguments.

```csharp
class C
{
    private int _x;

    public int X
    {
        get { return _x; }
        set { _x = value; }
    }

    public int Y() => _x;
}
```
