using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseUtf8StringLiteralAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp11Rules.UseUtf8StringLiteralRule);

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
            return;

        if (arrayCreation.Initializer == null)
            return;

        var bytes = arrayCreation.Initializer.Expressions
            .Select(TryGetByteConstant)
            .ToArray();

        if (bytes.Any(b => b == null))
            return;

        var byteArray = bytes.Select(b => b!.Value).ToArray();

        if (!TryDecodeAscii(byteArray))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp11Rules.UseUtf8StringLiteralRule,
            arrayCreation.GetLocation()));
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

        if (context.SemanticModel.GetSymbolInfo(memberAccess, context.CancellationToken).Symbol is not IMethodSymbol symbol)
            return;

        if (symbol.ContainingType.ToDisplayString() != "System.Text.Encoding")
            return;

        // Ensure receiver is Encoding.UTF8
        if (memberAccess.Expression is not MemberAccessExpressionSyntax receiver ||
            receiver.Name.Identifier.ValueText != "UTF8")
            return;

        var receiverSymbol = context.SemanticModel.GetSymbolInfo(receiver, context.CancellationToken).Symbol;
        if (receiverSymbol is not IPropertySymbol prop ||
            prop.ContainingType.ToDisplayString() != "System.Text.Encoding")
            return;

        // Only suggest for ASCII subset to keep it simple.
        var bytes = Encoding.UTF8.GetBytes(s);
        if (!TryDecodeAscii(bytes))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp11Rules.UseUtf8StringLiteralRule, invocation.GetLocation()));
    }

    private static byte? TryGetByteConstant(ExpressionSyntax expr)
    {
        if (expr is not LiteralExpressionSyntax literal) return null;
        return literal.Token.Value switch
        {
            byte b => b,
            int i and >= 0 and <= 255 => (byte)i,
            _ => null
        };
    }

    private static bool TryDecodeAscii(byte[] bytes)
    {
        // Require printable ASCII (plus common whitespace).
        if (bytes.Where(b => b != 0x09 && b != 0x0A && b != 0x0D).Any(b => b is < 0x20 or > 0x7E))
        {
            return false;
        }

        _ = Encoding.ASCII.GetString(bytes);
        return true;
    }
}