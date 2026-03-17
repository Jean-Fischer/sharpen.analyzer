using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Extensions;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class SyntaxNodeExtensionsObjectInitializerTests
{
    [Fact]
    public void IsObjectInitializerNamedAssignmentIdentifier_WhenObjectCreationInitializer_ReturnsTrueAndInitializedInstanceIsObjectCreation()
    {
        var root = Parse(@"
class C
{
    void M()
    {
        var c = new C { P = 1 };
    }

    public int P { get; set; }
}");

        var identifier = root.DescendantNodes().OfType<IdentifierNameSyntax>().Single(n => n.Identifier.ValueText == "P");

        var result = ((SyntaxNode)identifier).IsObjectInitializerNamedAssignmentIdentifier(out var initializedInstance);

        Assert.True(result);
        Assert.NotNull(initializedInstance);
        Assert.IsType<ObjectCreationExpressionSyntax>(initializedInstance);
    }

    [Fact]
    public void IsObjectInitializerNamedAssignmentIdentifier_WhenAssignmentInitializer_ReturnsTrueAndInitializedInstanceIsAssignmentLeft()
    {
        var root = Parse(@"
class C
{
    void M()
    {
        C c;
        c = new C { P = 1 };
    }

    public int P { get; set; }
}");

        var identifier = root.DescendantNodes().OfType<IdentifierNameSyntax>().Single(n => n.Identifier.ValueText == "P");

        var result = ((SyntaxNode)identifier).IsObjectInitializerNamedAssignmentIdentifier(out var initializedInstance);

        Assert.True(result);
        Assert.NotNull(initializedInstance);

        var assignment = root.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Single(a => a.IsKind(SyntaxKind.SimpleAssignmentExpression) && a.Left is IdentifierNameSyntax left && left.Identifier.ValueText == "c");
        Assert.Same(assignment.Left, initializedInstance);
    }

    [Fact]
    public void IsObjectInitializerNamedAssignmentIdentifier_WhenNotInObjectInitializer_ReturnsFalseAndInitializedInstanceIsNull()
    {
        var root = Parse(@"
class C
{
    void M()
    {
        var c = new C();
        c.P = 1;
    }

    public int P { get; set; }
}");

        var identifier = root.DescendantNodes().OfType<IdentifierNameSyntax>().Single(n => n.Identifier.ValueText == "P");

        var result = ((SyntaxNode)identifier).IsObjectInitializerNamedAssignmentIdentifier(out var initializedInstance);

        Assert.False(result);
        Assert.Null(initializedInstance);
    }

    private static CompilationUnitSyntax Parse(string code)
    {
        return CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();
    }
}
