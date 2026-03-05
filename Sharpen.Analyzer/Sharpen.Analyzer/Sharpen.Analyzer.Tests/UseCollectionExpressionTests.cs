using System.Threading.Tasks;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Sharpen.Analyzer.Analyzers.UseCollectionExpressionAnalyzer>;

namespace Sharpen.Analyzer.Tests;

public class UseCollectionExpressionTests
{
    [Fact]
    public async Task UseCollectionExpression_Triggers_ForExplicitArrayCreationWithInitializer()
    {
        const string original = @"
public class C
{
    public int[] M()
    {
        return new int[] { 1, 2, 3 };
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 16);
        await Verifier.VerifyAnalyzerAsync(original, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCollectionExpression_Triggers_ForImplicitArrayCreationWithInitializer()
    {
        const string original = @"
public class C
{
    public int[] M()
    {
        return new[] { 1, 2, 3 };
    }
}
";

        var expected = Verifier.Diagnostic().WithLocation(6, 16);
        await Verifier.VerifyAnalyzerAsync(original, expected).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCollectionExpression_DoesNotTrigger_ForExplicitSizedArrayCreation()
    {
        const string code = @"
public class C
{
    public int[] M()
    {
        return new int[3] { 1, 2, 3 };
    }
}
";

        await Verifier.VerifyAnalyzerAsync(code).ConfigureAwait(false);
    }
}
