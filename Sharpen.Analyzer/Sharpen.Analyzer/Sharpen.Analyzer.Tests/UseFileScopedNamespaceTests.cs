using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseFileScopedNamespaceAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseFileScopedNamespaceCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseFileScopedNamespaceTests
{
    [Fact]
    public async Task UseFileScopedNamespace_TriggersAndFixes_ForSingleNamespaceWithMultipleMembers()
    {
        var source = @"
namespace MyNs
{
    class A { }
    struct B { }
    interface I { }
}
";

        var fixedSource = @"
namespace MyNs;

class A { }
struct B { }
interface I { }
";

        await Verifier.VerifyCodeFixAsync(
            source,
            new[]
            {
                Verifier.Diagnostic("SHARPEN040").WithSpan(2, 11, 2, 15).WithSeverity(DiagnosticSeverity.Info)
            },
            fixedSource);
    }

    [Fact]
    public async Task UseFileScopedNamespace_TriggersAndFixes_ForNestedNamespaces()
    {
        var source = @"
namespace Outer
{
    namespace Inner
    {
        class C { }
    }
}
";

        // File-scoped namespaces cannot contain nested namespace declarations.
        // The fix lifts the outer namespace to file-scoped and moves the inner namespace to the compilation unit.
        var fixedSource = @"
namespace Outer;

namespace Inner
{
    class C { }
}
";

        // The code fix is not offered for nested namespaces (file-scoped namespaces cannot contain nested namespaces).
        await Verifier.VerifyAnalyzerAsync(
            source, Verifier.Diagnostic("SHARPEN040").WithSpan(2, 11, 2, 16).WithSeverity(DiagnosticSeverity.Info));
    }

    [Fact]
    public async Task UseFileScopedNamespace_DoesNotTrigger_WhenTwoSiblingNamespacesExist()
    {
        var source = @"
namespace A
{
    class A1 { }
}

namespace B
{
    class B1 { }
}
";

        await Verifier.VerifyAnalyzerAsync(source);
    }
}