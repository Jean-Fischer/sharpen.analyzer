using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Extensions;

public static class CSharp12SyntaxFactory
{
    public static CollectionExpressionSyntax CollectionExpression(SeparatedSyntaxList<CollectionElementSyntax> elements)
    {
        return SyntaxFactory.CollectionExpression(SyntaxFactory.Token(SyntaxKind.OpenBracketToken), elements,
            SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
    }

    public static AttributeSyntax InlineArrayAttribute(int length)
    {
        return SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("System.Runtime.CompilerServices.InlineArray"),
            SyntaxFactory.AttributeArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                        SyntaxFactory.Literal(length))))));
    }
}