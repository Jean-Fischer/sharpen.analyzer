using System.Threading.Tasks;
using Xunit;
using VerifierReturn = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInReturnStatementsAnalyzer>;
using VerifierMethod = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInOptionalMethodParametersAnalyzer>;
using VerifierCtor = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseDefaultExpressionInOptionalConstructorParametersAnalyzer>;

namespace Sharpen.Analyzer.Tests
{
    public sealed class UseDefaultExpressionAnalyzerTests
    {
        [Fact]
        public async Task Reports_diagnostic_for_default_expression_in_return_statement()
        {
            const string test = @"
class C
{
    int M()
    {
        return default(int);
    }
}";

            // Diagnostic is reported on the `default(int)` expression.
            var expected = VerifierReturn.Diagnostic(Rules.Rules.UseDefaultExpressionInReturnStatementsRule)
                .WithSpan(6, 16, 6, 28)
                .WithArguments("int");

            await VerifierReturn.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task Reports_diagnostic_for_default_expression_in_optional_method_parameter()
        {
            const string test = @"
class C
{
    void M(int x = default(int)) { }
}";

            var expected = VerifierMethod.Diagnostic(Rules.Rules.UseDefaultExpressionInOptionalMethodParametersRule)
                .WithSpan(4, 20, 4, 27)
                .WithArguments("int");

            await VerifierMethod.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
        }

        [Fact]
        public async Task Reports_diagnostic_for_default_expression_in_optional_constructor_parameter()
        {
            const string test = @"
class C
{
    public C(int x = default(int)) { }
}";

            var expected = VerifierCtor.Diagnostic(Rules.Rules.UseDefaultExpressionInOptionalConstructorParametersRule)
                .WithSpan(4, 22, 4, 29)
                .WithArguments("int");

            await VerifierCtor.VerifyAnalyzerAsync(test, expected).ConfigureAwait(false);
        }
    }
}
