using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp9.UseTargetTypedNewAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp9.UseTargetTypedNewCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseTargetTypedNewTests
{
    [Fact]
    public async Task UseTargetTypedNew_TriggersAndFixes_ForExplicitLocalInitializer()
    {
        const string original = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        List<int> xs = new List<int>();
    }
}
";

        const string fixedCode = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        List<int> xs = new();
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(8, 24);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }

    [Fact]
    public async Task UseTargetTypedNew_TriggersAndFixes_ForPropertyInitializer_WithObjectInitializer()
    {
        const string original = @"
using System.Collections.Generic;

public class C
{
    public List<int> Xs { get; } = new List<int> { 1, 2, 3 };
}
";

        const string fixedCode = @"
using System.Collections.Generic;

public class C
{
    public List<int> Xs { get; } = new() { 1, 2, 3 };
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 36);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }

    [Fact]
    public async Task UseTargetTypedNew_TriggersAndFixes_ForAssignment_WhenTypesMatch()
    {
        const string original = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        List<int> xs;
        xs = new List<int>();
    }
}
";

        const string fixedCode = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        List<int> xs;
        xs = new();
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(9, 14);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }

    [Fact]
    public async Task UseTargetTypedNew_TriggersAndFixes_ForReturn_WhenTypesMatch()
    {
        const string original = @"
using System.Collections.Generic;

public class C
{
    public List<int> M()
    {
        return new List<int>();
    }
}
";

        const string fixedCode = @"
using System.Collections.Generic;

public class C
{
    public List<int> M()
    {
        return new();
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(8, 16);
        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode);
    }

    [Fact]
    public async Task UseTargetTypedNew_DoesNotTrigger_ForVarDeclaration()
    {
        const string code = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        var xs = new List<int>();
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task UseTargetTypedNew_DoesNotTrigger_WhenDeclaredTypeIsInterface()
    {
        const string code = @"
using System.Collections.Generic;

public class C
{
    public void M()
    {
        ICollection<object> test = new List<object>();
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code);
    }
}
