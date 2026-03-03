using System.Threading.Tasks;
using Xunit;
using VerifierReturn = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInReturnStatementsAnalyzer,
    Sharpen.Analyzer.FixProvider.UseDefaultExpressionCodeFixProvider>;
using VerifierMethod = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInOptionalMethodParametersAnalyzer,
    Sharpen.Analyzer.FixProvider.UseDefaultExpressionCodeFixProvider>;
using VerifierCtor = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInOptionalConstructorParametersAnalyzer,
    Sharpen.Analyzer.FixProvider.UseDefaultExpressionCodeFixProvider>;

namespace Sharpen.Analyzer.Tests
{
    public sealed class UseDefaultExpressionCodeFixTests
    {
        [Fact]
        public async Task Fixes_default_expression_in_return_statement()
        {
            const string test = @"
class C
{
    int M()
    {
        return default(int);
    }
}";

            const string fixedCode = @"
class C
{
    int M()
    {
        return default;
    }
}";

            var expected = VerifierReturn.Diagnostic(Rules.Rules.UseDefaultExpressionInReturnStatementsRule)
                .WithSpan(6, 9, 6, 15)
                .WithArguments("int");

            await VerifierReturn.VerifyCodeFixAsync(test, expected, fixedCode).ConfigureAwait(false);
        }

        [Fact]
        public async Task Fixes_default_expression_in_optional_method_parameter()
        {
            const string test = @"
class C
{
    void M(int x = default(int)) { }
}";

            const string fixedCode = @"
class C
{
    void M(int x = default) { }
}";

            var expected = VerifierMethod.Diagnostic(Rules.Rules.UseDefaultExpressionInOptionalMethodParametersRule)
                .WithSpan(4, 20, 4, 27)
                .WithArguments("int");

            await VerifierMethod.VerifyCodeFixAsync(test, expected, fixedCode).ConfigureAwait(false);
        }

        [Fact]
        public async Task Fixes_default_expression_in_optional_constructor_parameter()
        {
            const string test = @"
class C
{
    public C(int x = default(int)) { }
}";

            const string fixedCode = @"
class C
{
    public C(int x = default) { }
}";

            var expected = VerifierCtor.Diagnostic(Rules.Rules.UseDefaultExpressionInOptionalConstructorParametersRule)
                .WithSpan(4, 22, 4, 29)
                .WithArguments("int");

            await VerifierCtor.VerifyCodeFixAsync(test, expected, fixedCode).ConfigureAwait(false);
        }
    }
}
