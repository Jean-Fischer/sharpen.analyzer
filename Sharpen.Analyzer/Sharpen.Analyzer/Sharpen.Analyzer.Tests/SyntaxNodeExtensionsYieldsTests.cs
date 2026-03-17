using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Extensions;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class SyntaxNodeExtensionsYieldsTests
{
    [Fact]
    public void Yields_WhenYieldStatementIsDirectlyInMethodBody_ReturnsTrue()
    {
        var root = Parse(@"
using System.Collections.Generic;

class C
{
    IEnumerable<int> M()
    {
        yield return 1;
    }
}");

        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        Assert.True(((SyntaxNode)method).Yields());
    }

    [Fact]
    public void Yields_WhenYieldStatementIsInsideNestedLocalFunction_ReturnsFalseForOuterMethod()
    {
        var root = Parse(@"
using System.Collections.Generic;

class C
{
    IEnumerable<int> M()
    {
        IEnumerable<int> Local()
        {
            yield return 1;
        }

        return Local();
    }
}");

        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        Assert.False(((SyntaxNode)method).Yields());
    }

    [Fact]
    public void Yields_WhenYieldStatementIsInsideLambda_ReturnsFalseForEnclosingMethod()
    {
        var root = Parse(@"
 using System;
 using System.Collections.Generic;
 
 class C
 {
     IEnumerable<int> M()
     {
         Func<IEnumerable<int>> f = () =>
         {
             yield return 1;
         };
 
         return f();
     }
 }");
 
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
 
        Assert.False(((SyntaxNode)method).Yields());
    }

    [Fact]
    public void FirstAncestorOrSelfWithinEnclosingNode_WhenNotFound_ReturnsNull()
    {
        var root = Parse(@"
 using System.Collections.Generic;
 
 class C
 {
     IEnumerable<int> M()
     {
         yield return 1;
     }
 }");

        var yieldStatement = root.DescendantNodes().OfType<YieldStatementSyntax>().Single();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

        var enclosingAnonymousFunction =
            yieldStatement.FirstAncestorOrSelfWithinEnclosingNode<AnonymousFunctionExpressionSyntax>(method,
                includeEnclosingNode: false);

        Assert.Null(enclosingAnonymousFunction);
    }

    private static CompilationUnitSyntax Parse(string code)
    {
        return CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();
    }
}
