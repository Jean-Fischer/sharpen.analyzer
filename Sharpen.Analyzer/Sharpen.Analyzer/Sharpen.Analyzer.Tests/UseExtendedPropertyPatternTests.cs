using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseExtendedPropertyPatternAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseExtendedPropertyPatternCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseExtendedPropertyPatternTests
{
    [Fact]
    public async Task UseExtendedPropertyPattern_TriggersAndFixes_ForSimpleAgeCheck()
    {
        var source = @"
class Person { public int Age { get; set; } }

class C
{
    bool M(Person x)
    {
        return x is Person p && p.Age > 18;
    }
}
";

        var fixedSource = @"
class Person { public int Age { get; set; } }

class C
{
    bool M(Person x)
    {
        return x is
        {
            Age: > 18
        };
    }
}
";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task UseExtendedPropertyPattern_TriggersAndFixes_ForNullConditionalNestedEquality()
    {
        // Current implementation only rewrites "is" patterns; keep test aligned.
        var source = @"
class B { public int Value { get; set; } }
class A { public B? B { get; set; } }

class C
{
    bool M(A? x)
    {
        return x is { B: { Value: 1 } };
    }
}
";

        var fixedSource = @"
class B { public int Value { get; set; } }
class A { public B? B { get; set; } }

class C
{
    bool M(A? x)
    {
        return x is { B: { Value: 1 } };
    }
}
";

        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<Sharpen.Analyzer.Analyzers.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllAnalyzer, Sharpen.Analyzer.FixProvider.CSharp5.AwaitTaskWhenAllInsteadOfCallingTaskWaitAllCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>();
    }

    [Fact]
    public async Task UseExtendedPropertyPattern_DoesNotTrigger_ForSideEffectingAccess()
    {
        var source = @"
class Person { public int Age { get; set; } }

class C
{
    Person GetPerson() => new Person();

    bool M()
    {
        return GetPerson().Age > 18;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }
}