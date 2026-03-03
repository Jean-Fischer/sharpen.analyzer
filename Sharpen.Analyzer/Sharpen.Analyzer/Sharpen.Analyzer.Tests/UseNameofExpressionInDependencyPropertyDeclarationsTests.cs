using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseNameofExpressionInDependencyPropertyDeclarationsAnalyzer,
    Sharpen.Analyzer.FixProvider.UseNameofExpressionInDependencyPropertyDeclarationsCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public sealed class UseNameofExpressionInDependencyPropertyDeclarationsTests
{
    [Fact]
    public async Task ReportsDiagnostic_For_Register_WhenStringLiteralMatchesProperty()
    {
        const string test = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.Register(""Foo"", typeof(int), typeof(C));

    public int Foo { get; }
}";

        var expected = Verifier.Diagnostic().WithSpan(13, 52, 13, 57);
        await Verifier.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReportsDiagnostic_For_RegisterAttached_WhenStringLiteralMatchesProperty()
    {
        const string test = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty RegisterAttached(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.RegisterAttached(""Foo"", typeof(int), typeof(C));

    public int Foo { get; }
}";

        var expected = Verifier.Diagnostic().WithSpan(13, 60, 13, 65);
        await Verifier.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenStringLiteralDoesNotMatchAnyProperty()
    {
        const string test = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.Register(""Bar"", typeof(int), typeof(C));

    public int Foo { get; }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task DoesNotReportDiagnostic_WhenNameofAlreadyUsed()
    {
        const string test = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.Register(nameof(Foo), typeof(int), typeof(C));

    public int Foo { get; }
}";

        await Verifier.VerifyAnalyzerAsync(test).ConfigureAwait(false);
    }

    [Fact]
    public async Task CodeFix_ReplacesStringLiteralWithNameof()
    {
        const string test = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.Register(""Foo"", typeof(int), typeof(C));

    public int Foo { get; }
}";

        const string fixedCode = @"
namespace System.Windows
{
    public class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => null;
    }
}

class C
{
    public static readonly System.Windows.DependencyProperty FooProperty =
        System.Windows.DependencyProperty.Register(nameof(Foo), typeof(int), typeof(C));

    public int Foo { get; }
}";

        var expected = Verifier.Diagnostic().WithSpan(13, 52, 13, 57);
        await Verifier.VerifyCodeFixAsync(test, expected, fixedCode).ConfigureAwait(false);
    }
}
