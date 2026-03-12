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

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseUnboundGenericTypeInNameofCodeFixProvider))]
[Shared]
public sealed class UseUnboundGenericTypeInNameofCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseUnboundGenericTypeInNameofRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        if (node is not TypeSyntax typeSyntax)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            checker: new UnboundGenericTypeInNameofSafetyChecker(),
            syntaxTree: root.SyntaxTree,
            semanticModel: semanticModel,
            diagnostic: diagnostic,
            matchSucceeded: true,
            cancellationToken: context.CancellationToken);

        if (safetyEvaluation.Outcome != FixProviderSafetyOutcome.Safe)
            return;

        RegisterCodeFix(
            context: context,
            diagnostic: diagnostic,
            title: "Use unbound generic type in nameof",
            equivalenceKey: nameof(UseUnboundGenericTypeInNameofCodeFixProvider),
            createChangedDocument: ct => ApplyAsync(context.Document, typeSyntax, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, TypeSyntax typeSyntax, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var currentType = root.FindNode(typeSyntax.Span, getInnermostNodeForTie: true) as TypeSyntax;
        if (currentType is null)
            return document;

        if (!TryCreateUnboundTypeSyntax(currentType, out var unboundType))
            return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        editor.ReplaceNode(currentType, unboundType.WithTriviaFrom(currentType));
        return editor.GetChangedDocument();
    }

    private static bool TryCreateUnboundTypeSyntax(TypeSyntax typeSyntax, out TypeSyntax unboundType)
    {
        unboundType = null!;

        switch (typeSyntax)
        {
            case GenericNameSyntax genericName:
                unboundType = MakeUnboundGenericName(genericName);
                return true;

            case QualifiedNameSyntax qualifiedName when qualifiedName.Right is GenericNameSyntax rightGeneric:
                unboundType = qualifiedName.WithRight(MakeUnboundGenericName(rightGeneric));
                return true;

            case AliasQualifiedNameSyntax aliasQualifiedName when aliasQualifiedName.Name is GenericNameSyntax aliasGeneric:
                unboundType = aliasQualifiedName.WithName(MakeUnboundGenericName(aliasGeneric));
                return true;

            default:
                return false;
        }
    }

    private static GenericNameSyntax MakeUnboundGenericName(GenericNameSyntax genericName)
    {
        var arity = genericName.TypeArgumentList.Arguments.Count;
        var omitted = new SeparatedSyntaxList<TypeSyntax>();
        for (var i = 0; i < arity; i++)
            omitted = omitted.Add(SyntaxFactory.OmittedTypeArgument());

        return genericName.WithTypeArgumentList(SyntaxFactory.TypeArgumentList(omitted));
    }
}
