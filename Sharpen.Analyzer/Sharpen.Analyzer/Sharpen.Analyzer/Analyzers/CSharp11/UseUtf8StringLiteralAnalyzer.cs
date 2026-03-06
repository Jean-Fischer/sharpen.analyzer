using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseUtf8StringLiteralAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp11Rules.UseUtf8StringLiteralRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp11OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeArrayCreation, SyntaxKind.ArrayCreationExpression);
            compilationContext.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeArrayCreation(SyntaxNodeAnalysisContext context)
    {
        var arrayCreation = (ArrayCreationExpressionSyntax)context.Node;

        // new byte[] { ... }
        if (arrayCreation.Type.ElementType is not PredefinedTypeSyntax predefined ||
            !predefined.Keyword.IsKind(SyntaxKind.ByteKeyword))
        {
            return;
        }

        if (arrayCreation.Initializer == null)
            return;

        var bytes = arrayCreation.Initializer.Expressions
            .Select(TryGetByteConstant)
            .ToArray();

        if (bytes.Any(b => b == null))
            return;

        var byteArray = bytes.Select(b => b!.Value).ToArray();

        if (!TryDecodeAscii(byteArray, out _))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp11Rules.UseUtf8StringLiteralRule, arrayCreation.GetLocation()));
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Encoding.UTF8.GetBytes("...")
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.ValueText != "GetBytes")
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argExpr = invocation.ArgumentList.Arguments[0].Expression;
        var constant = context.SemanticModel.GetConstantValue(argExpr, context.CancellationToken);
        if (!constant.HasValue || constant.Value is not string s)
            return;

        var symbol = context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol as IMethodSymbol;
        if (symbol == null)
            return;

        if (symbol.ContainingType.ToDisplayString() != "System.Text.Encoding")
            return;

        // Ensure receiver is Encoding.UTF8
        if (memberAccess.Expression is not MemberAccessExpressionSyntax receiver || receiver.Name.Identifier.ValueText != "UTF8")
            return;

        var receiverSymbol = context.SemanticModel.GetSymbolInfo(receiver, context.CancellationToken).Symbol;
        if (receiverSymbol is not IPropertySymbol prop || prop.ContainingType.ToDisplayString() != "System.Text.Encoding")
            return;

        // Only suggest for ASCII subset to keep it simple.
        var bytes = Encoding.UTF8.GetBytes(s);
        if (!TryDecodeAscii(bytes, out _))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp11Rules.UseUtf8StringLiteralRule, invocation.GetLocation()));
    }

    private static byte? TryGetByteConstant(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax literal)
        {
            if (literal.Token.Value is byte b)
                return b;

            if (literal.Token.Value is int i && i is >= 0 and <= 255)
                return (byte)i;
        }

        return null;
    }

    private static bool TryDecodeAscii(byte[] bytes, out string text)
    {
        text = string.Empty;

        // Require printable ASCII (plus common whitespace).
        foreach (var b in bytes)
        {
            if (b == 0x09 || b == 0x0A || b == 0x0D)
                continue;

            if (b < 0x20 || b > 0x7E)
                return false;
        }

        text = Encoding.ASCII.GetString(bytes);
        return true;
    }
}
