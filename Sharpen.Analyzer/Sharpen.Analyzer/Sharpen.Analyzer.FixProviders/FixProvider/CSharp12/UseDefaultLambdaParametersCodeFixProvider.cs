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

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
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
                title: "Use default lambda parameters",
                createChangedDocument: ct => UseDefaultLambdaParametersAsync(context.Document, lambda, ct),
                equivalenceKey: nameof(UseDefaultLambdaParametersCodeFixProvider)),
            diagnostic);
    }

    private static async Task<bool> CanApplyFixAsync(Document document, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
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

            _ => false,
        };
    }

    private static async Task<Document> UseDefaultLambdaParametersAsync(Document document, LambdaExpressionSyntax lambda, CancellationToken cancellationToken)
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
            {
                if (invoke.Parameters.Length != 1)
                    return document;

                var updatedParameter = AddDefaultValue(simple.Parameter, invoke.Parameters[0]);
                if (updatedParameter is null)
                    return document;

                editor.ReplaceNode(simple.Parameter, updatedParameter);
                break;
            }

            case ParenthesizedLambdaExpressionSyntax parenthesized:
            {
                if (invoke.Parameters.Length != parenthesized.ParameterList.Parameters.Count)
                    return document;

                var parameters = parenthesized.ParameterList.Parameters;
                var updated = parameters
                    .Select((p, i) => AddDefaultValue(p, invoke.Parameters[i]) ?? p)
                    .ToArray();

                var updatedList = parenthesized.ParameterList.WithParameters(SyntaxFactory.SeparatedList(updated, parenthesized.ParameterList.Parameters.GetSeparators()));
                editor.ReplaceNode(parenthesized.ParameterList, updatedList);
                break;
            }
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
        ExpressionSyntax defaultValueExpression;
        if (parameterSymbol.ExplicitDefaultValue is null)
        {
            defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }
        else
        {
            defaultValueExpression = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(parameterSymbol.ExplicitDefaultValue.ToString()));

            // For non-numeric constants, use generator to avoid formatting issues.
            // We'll replace below if needed.
            if (parameterSymbol.ExplicitDefaultValue is bool b)
                defaultValueExpression = b ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            else if (parameterSymbol.ExplicitDefaultValue is string s)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
            else if (parameterSymbol.ExplicitDefaultValue is char c)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(c));
            else if (parameterSymbol.ExplicitDefaultValue is int i)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
            else if (parameterSymbol.ExplicitDefaultValue is long l)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(l));
            else if (parameterSymbol.ExplicitDefaultValue is double d)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
            else if (parameterSymbol.ExplicitDefaultValue is float f)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(f));
            else if (parameterSymbol.ExplicitDefaultValue is decimal m)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(m));
            else if (parameterSymbol.ExplicitDefaultValue is byte bt)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(bt));
            else if (parameterSymbol.ExplicitDefaultValue is sbyte sb)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(sb));
            else if (parameterSymbol.ExplicitDefaultValue is short sh)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(sh));
            else if (parameterSymbol.ExplicitDefaultValue is ushort ush)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ush));
            else if (parameterSymbol.ExplicitDefaultValue is uint ui)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ui));
            else if (parameterSymbol.ExplicitDefaultValue is ulong ul)
                defaultValueExpression = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(ul));
            else
                return null;
        }

        return parameterSyntax.WithDefault(
            SyntaxFactory.EqualsValueClause(defaultValueExpression)
                .WithLeadingTrivia(SyntaxFactory.Space));
    }
}
