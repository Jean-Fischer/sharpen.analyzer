using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp9.UseInitOnlySetterAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp9.UseInitOnlySetterCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseInitOnlySetterTests
{
    // The Roslyn test harness compiles the test code in an in-memory project.
    // Add the IsExternalInit polyfill so init-only setters compile.
    private const string IsExternalInitPolyfill = @"
namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit { }
}
";

    [Fact]
    public async Task UseInitOnlySetter_TriggersAndFixes_ForPrivateSetAutoProperty()
    {
        var original = IsExternalInitPolyfill + @"
public class Person
{
    public string Name { get; private set; }

    public Person(string name)
    {
        Name = name;
    }
}
";

        var fixedCode = IsExternalInitPolyfill + @"
public class Person
{
    public string Name { get; init; }

    public Person(string name)
    {
        Name = name;
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(9, 19);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }

    [Fact]
    public async Task UseInitOnlySetter_DoesNotTrigger_WhenAssignedOutsideConstructor()
    {
        var code = IsExternalInitPolyfill + @"
public class Person
{
    public string Name { get; private set; }

    public Person(string name)
    {
        Name = name;
    }

    public void Rename(string name)
    {
        Name = name;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task UseInitOnlySetter_DoesNotTrigger_ForNonTrivialSetter()
    {
        var code = IsExternalInitPolyfill + @"
public class Person
{
    private string _name;

    public string Name
    {
        get => _name;
        private set => _name = value;
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task UseInitOnlySetter_PreservesAccessorAttributes()
    {
        var original = @"
using System;

" + IsExternalInitPolyfill + @"

public class Person
{
    public string Name { get; [Obsolete] private set; }
}
";

        var fixedCode = @"
using System;

" + IsExternalInitPolyfill + @"

public class Person
{
    public string Name { get; [Obsolete] init; }
}
";

        var expected = Verifier.Diagnostic().WithLocation(13, 19);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }
}
