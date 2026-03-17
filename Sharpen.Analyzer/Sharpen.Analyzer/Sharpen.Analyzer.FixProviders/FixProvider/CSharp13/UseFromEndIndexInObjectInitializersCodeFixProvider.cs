using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFromEndIndexInObjectInitializersCodeFixProvider))]
[Shared]
public sealed class UseFromEndIndexInObjectInitializersCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.UseFromEndIndexInObjectInitializersRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Diagnostic is reported on the index expression.
        var indexExpression = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ExpressionSyntax;
        if (indexExpression is null)
            return;

        // In object initializers, the indexer assignment uses ImplicitElementAccessSyntax.
        // FindNode may return a nested node (e.g. the `Length` member access),
        // so walk up until we reach the full `<expr>.Length - 1` expression.
        while (indexExpression.Parent is ExpressionSyntax parentExpression &&
               indexExpression.Parent is not ArgumentSyntax)
            indexExpression = parentExpression;

        if (indexExpression.Parent is not ArgumentSyntax
            {
                Parent: BracketedArgumentListSyntax { Parent: ImplicitElementAccessSyntax }
            })
            return;

        if (!IsLengthMinusOne(indexExpression))
            return;

        // Ensure the `.Length` is from an array (keep analyzer + fix consistent).
        if (indexExpression is not BinaryExpressionSyntax { Left: MemberAccessExpressionSyntax memberAccess })
            return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var lengthTargetType = semanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (lengthTargetType is null || lengthTargetType.TypeKind != TypeKind.Array)
            return;

        // Safety checker already validates the same conditions; keep it as the final gate.
        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new UseFromEndIndexInObjectInitializersSafetyChecker(),
            root.SyntaxTree,
            semanticModel,
            diagnostic,
            true,
            context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        RegisterCodeFix(
            context,
            diagnostic,
            "Use from-end index (^) in initializer",
            nameof(UseFromEndIndexInObjectInitializersCodeFixProvider),
            ct => ApplyAsync(context.Document, indexExpression, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, ExpressionSyntax indexExpression,
        CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Replace `<expr>.Length - 1` with `^1`
        var fromEnd = PrefixUnaryExpression(
            SyntaxKind.IndexExpression,
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)));

        editor.ReplaceNode(indexExpression, fromEnd.WithTriviaFrom(indexExpression));

        return editor.GetChangedDocument();
    }

    private static bool IsLengthMinusOne(ExpressionSyntax expression)
    {
        if (expression is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.SubtractExpression } subtract)
            return false;

        if (subtract.Right is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NumericLiteralExpression } literal)
            return false;

        if (literal.Token.ValueText != "1")
            return false;

        if (subtract.Left is not MemberAccessExpressionSyntax memberAccess)
            return false;

        return memberAccess.Name.Identifier.ValueText == "Length";
    }
}