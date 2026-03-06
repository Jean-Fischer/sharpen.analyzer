using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Sharpen.Analyzer.Rules;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.CodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp12.UseCollectionExpressionAnalyzer,
    Sharpen.Analyzer.UseCollectionExpressionCodeFixProvider>;

namespace Sharpen.Analyzer.Tests;

public class UseCollectionExpressionCodeFixTests
{
    [Fact]
    public async Task UseCollectionExpression_Fixes_ExplicitArrayCreationWithInitializer()
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

        const string fixedCode = @"
public class C
{
    public int[] M()
    {
        return [1, 2, 3];
    }
}
";

        var expected = Verifier.Diagnostic(CSharp12Rules.UseCollectionExpressionRule)
            .WithLocation(6, 16);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCollectionExpression_Fixes_ImplicitArrayCreationWithInitializer()
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

        const string fixedCode = @"
public class C
{
    public int[] M()
    {
        return [1, 2, 3];
    }
}
";

        var expected = Verifier.Diagnostic(CSharp12Rules.UseCollectionExpressionRule)
            .WithLocation(6, 16);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCollectionExpression_Fix_PreservesElementTrivia()
    {
        const string original = @"
public class C
{
    public int[] M()
    {
        return new int[]
        {
            1, // a
            2,
            3,
        };
    }
}
";

        const string fixedCode = @"
public class C
{
    public int[] M()
    {
        return [1, 2, 3];
    }
}
";

        var expected = Verifier.Diagnostic(CSharp12Rules.UseCollectionExpressionRule)
            .WithLocation(6, 16);

        await Verifier.VerifyCodeFixAsync(original, expected, fixedCode).ConfigureAwait(false);
    }

    [Fact]
    public async Task UseCollectionExpression_DoesNotOfferFix_ForExplicitSizedArrayCreation()
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
