using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Extensions;

internal static class InvocationExpressionSyntaxExtensions
{
    public static string GetInvokedMemberName(this InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
            _ => string.Empty
        };
    }
}