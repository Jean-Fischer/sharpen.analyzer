using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp12;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseDefaultLambdaParametersCodeFixProvider))]
[Shared]
public sealed class UseDefaultLambdaParametersCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UseDefaultLambdaParametersRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        var lambda = node.FirstAncestorOrSelf<LambdaExpressionSyntax>();
        if (lambda is null)
            return;

        // Avoid registering a no-op fix (e.g. parameterless lambdas like (() => ...)).
        if (!await CanApplyFixAsync(context.Document, lambda, context.CancellationToken).ConfigureAwait(false))
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use default lambda parameters",
                ct => UseDefaultLambdaParametersAsync(context.Document, lambda, ct),
                nameof(UseDefaultLambdaParametersCodeFixProvider)),
            diagnostic);
    }

    private static async Task<bool> CanApplyFixAsync(Document document, LambdaExpressionSyntax lambda,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return false;

        if (semanticModel.GetTypeInfo(lambda, cancellationToken).ConvertedType is not INamedTypeSymbol delegateType)
            return false;

        if (delegateType.DelegateInvokeMethod is not IMethodSymbol invoke)
            return false;

        return lambda switch
        {
            SimpleLambdaExpressionSyntax simple =>
                invoke.Parameters.Length == 1 && AddDefaultValue(simple.Parameter, invoke.Parameters[0]) is not null,

            ParenthesizedLambdaExpressionSyntax parenthesized =>
                invoke.Parameters.Length == parenthesized.ParameterList.Parameters.Count
                && parenthesized.ParameterList.Parameters
                    .Select((p, i) => AddDefaultValue(p, invoke.Parameters[i]))
                    .Any(p => p is not null),

            _ => false
        };
    }

    private static async Task<Document> UseDefaultLambdaParametersAsync(Document document,
        LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        if (semanticModel.GetTypeInfo(lambda, cancellationToken).ConvertedType is not INamedTypeSymbol delegateType)
            return document;

        if (delegateType.DelegateInvokeMethod is not IMethodSymbol invoke)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        switch (lambda)
        {
            case SimpleLambdaExpressionSyntax simple:
                if (!TryUpdateSimpleLambda(editor, simple, invoke))
                    return document;
                break;
            case ParenthesizedLambdaExpressionSyntax parenthesized:
                if (!TryUpdateParenthesizedLambda(editor, parenthesized, invoke))
                    return document;
                break;
        }

        return editor.GetChangedDocument();
    }

    private static ParameterSyntax? AddDefaultValue(ParameterSyntax parameterSyntax, IParameterSymbol parameterSymbol)
    {
        if (!parameterSymbol.HasExplicitDefaultValue)
            return null;

        if (parameterSyntax.Default is not null)
            return null;

        // Only support constants and null (keep conservative).
        var defaultValueExpression = CreateDefaultValueExpression(parameterSymbol.ExplicitDefaultValue);
        if (defaultValueExpression is null)
            return null;

        return parameterSyntax.WithDefault(
            SyntaxFactory.EqualsValueClause(defaultValueExpression)
                .WithLeadingTrivia(SyntaxFactory.Space));
    }

    private static bool TryUpdateSimpleLambda(
        DocumentEditor editor,
        SimpleLambdaExpressionSyntax simple,
        IMethodSymbol invoke)
    {
        if (invoke.Parameters.Length != 1)
            return false;

        var updatedParameter = AddDefaultValue(simple.Parameter, invoke.Parameters[0]);
        if (updatedParameter is null)
            return false;

        editor.ReplaceNode(simple.Parameter, updatedParameter);
        return true;
    }

    private static bool TryUpdateParenthesizedLambda(
        DocumentEditor editor,
        ParenthesizedLambdaExpressionSyntax parenthesized,
        IMethodSymbol invoke)
    {
        if (invoke.Parameters.Length != parenthesized.ParameterList.Parameters.Count)
            return false;

        var parameters = parenthesized.ParameterList.Parameters;
        var updated = parameters
            .Select((p, i) => AddDefaultValue(p, invoke.Parameters[i]) ?? p)
            .ToArray();

        var updatedList = parenthesized.ParameterList.WithParameters(
            SyntaxFactory.SeparatedList(updated, parenthesized.ParameterList.Parameters.GetSeparators()));
        editor.ReplaceNode(parenthesized.ParameterList, updatedList);
        return true;
    }

    private static ExpressionSyntax? CreateDefaultValueExpression(object? value)
    {
        return value switch
        {
            null => SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression),
            bool b => b
                ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),
            string s => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s)),
            char c => SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(c)),
            int i => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i)),
            long l => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(l)),
            double d => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d)),
            float f => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f)),
            decimal m => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(m)),
            byte b => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(b)),
            sbyte sb => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(sb)),
            short sh => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(sh)),
            ushort ush => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ush)),
            uint ui => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ui)),
            ulong ul => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ul)),
            _ => null
        };
    }
}
