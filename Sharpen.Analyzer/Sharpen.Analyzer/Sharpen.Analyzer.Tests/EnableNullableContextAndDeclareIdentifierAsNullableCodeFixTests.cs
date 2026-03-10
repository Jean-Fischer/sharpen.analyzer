using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp8.EnableNullableContextAndDeclareIdentifierAsNullableAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp8.EnableNullableContextAndDeclareIdentifierAsNullableCodeFixProvider,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests
{
    public sealed class EnableNullableContextAndDeclareIdentifierAsNullableCodeFixTests
    {
        [Fact]
        public async Task Fixes_reference_type_field_assigned_null()
        {
            const string test = @"
class C
{
    private string _s;

    void M()
    {
        _s = null;
    }
}";

            const string fixedCode = @"
class C
{
    private string? _s;

    void M()
    {
        _s = null;
    }
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(8, 9, 8, 18);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }

        [Fact]
        public async Task Fixes_reference_type_local_assigned_null()
        {
            const string test = @"
class C
{
    void M()
    {
        string s = null;
    }
}";

            const string fixedCode = @"
class C
{
    void M()
    {
        string? s = null;
    }
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(6, 16, 6, 24);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }

        [Fact]
        public async Task Fixes_reference_type_property_initializer_assigned_null()
        {
            const string test = @"
class C
{
    public string P { get; set; } = null;
}";

            const string fixedCode = @"
class C
{
    public string? P { get; set; } = null;
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(4, 5, 4, 42);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }

        [Fact]
        public async Task Fixes_reference_type_optional_parameter_assigned_null()
        {
            const string test = @"
class C
{
    void M(string s = null) { }
}";

            const string fixedCode = @"
class C
{
    void M(string? s = null) { }
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(4, 12, 4, 27);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }

        [Fact]
        public async Task Fixes_reference_type_field_when_diagnostic_is_on_assignment_expression()
        {
            const string test = @"
class C
{
    private string _s;

    void M()
    {
        _s = null;
    }
}";

            const string fixedCode = @"
class C
{
    private string? _s;

    void M()
    {
        _s = null;
    }
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(8, 9, 8, 18);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }


        [Fact]
        public async Task Preserves_trivia_around_type()
        {
            const string test = @"
class C
{
    private /*t*/ string /*u*/ _s;

    void M()
    {
        _s = null;
    }
}";

            const string fixedCode = @"
class C
{
    private /*t*/ string? /*u*/ _s;

    void M()
    {
        _s = null;
    }
}";

            var expected = Verifier.Diagnostic(Sharpen.Analyzer.Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule)
                .WithSpan(8, 9, 8, 18);

            await Verifier.VerifyCodeFixAsync(test, expected, fixedCode);
        }

        [Fact]
        public async Task Does_not_offer_fix_when_type_is_already_nullable()
        {
            const string test = @"
class C
{
    private string? _s;

    void M()
    {
        _s = null;
    }
}";

            await Verifier.VerifyAnalyzerAsync(test);
        }
    }
}
