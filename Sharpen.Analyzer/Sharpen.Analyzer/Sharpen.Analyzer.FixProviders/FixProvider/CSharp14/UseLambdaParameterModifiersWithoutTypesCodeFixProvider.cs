using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseLambdaParameterModifiersWithoutTypesCodeFixProvider))]
[Shared]
public sealed class UseLambdaParameterModifiersWithoutTypesCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var parameterList = node as ParameterListSyntax ?? node.FirstAncestorOrSelf<ParameterListSyntax>();
        if (parameterList is null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            checker: new LambdaParameterModifiersWithoutTypesSafetyChecker(),
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
            title: CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule.Title.ToString(),
            equivalenceKey: nameof(UseLambdaParameterModifiersWithoutTypesCodeFixProvider),
            createChangedDocument: ct => ApplyFixAsync(context.Document, parameterList, ct));
    }

    private static async Task<Document> ApplyFixAsync(Document document, ParameterListSyntax parameterList, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var newParameters = parameterList.Parameters
            .Select(p => p.Type is null ? p : p.WithType(null))
            .ToList();

        var newParameterList = parameterList.WithParameters(new SeparatedSyntaxList<ParameterSyntax>().AddRange(newParameters));

        editor.ReplaceNode(parameterList, newParameterList);
        return editor.GetChangedDocument();
    }
}
