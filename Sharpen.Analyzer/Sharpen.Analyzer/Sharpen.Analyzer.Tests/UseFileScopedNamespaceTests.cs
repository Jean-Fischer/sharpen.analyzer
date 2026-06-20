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
        const string source = @"
namespace MyNs
{
    class A { }
    struct B { }
    interface I { }
}
";

        const string fixedSource = @"
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
        const string source = @"
namespace Outer
{
    namespace Inner
    {
        class C { }
    }
}
";

        // The code fix is not offered for nested namespaces (file-scoped namespaces cannot contain nested namespaces).
        await Verifier.VerifyAnalyzerAsync(
            source, Verifier.Diagnostic("SHARPEN040").WithSpan(2, 11, 2, 16).WithSeverity(DiagnosticSeverity.Info));
    }

    [Fact]
    public async Task UseFileScopedNamespace_DoesNotTrigger_WhenTwoSiblingNamespacesExist()
    {
        const string source = @"
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
