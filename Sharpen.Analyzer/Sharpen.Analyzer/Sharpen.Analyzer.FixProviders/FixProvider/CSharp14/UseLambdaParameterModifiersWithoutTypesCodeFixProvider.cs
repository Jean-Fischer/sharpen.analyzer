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

namespace Sharpen.Analyzer.FixProvider.CSharp14;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseLambdaParameterModifiersWithoutTypesCodeFixProvider))]
[Shared]
public sealed class UseLambdaParameterModifiersWithoutTypesCodeFixProvider
    : CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<ParameterListSyntax,
        LambdaParameterModifiersWithoutTypesSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule.Id);

    protected override ParameterListSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node as ParameterListSyntax ?? node.FirstAncestorOrSelf<ParameterListSyntax>();
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        ParameterListSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule.Title.ToString(),
            nameof(UseLambdaParameterModifiersWithoutTypesCodeFixProvider),
            ct => ApplyFixAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyFixAsync(Document document, ParameterListSyntax parameterList,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        var newParameters = parameterList.Parameters
            .Select(p => p.Type is null ? p : p.WithType(null))
            .ToList();

        var newParameterList =
            parameterList.WithParameters(new SeparatedSyntaxList<ParameterSyntax>().AddRange(newParameters));

        editor.ReplaceNode(parameterList, newParameterList);
        return editor.GetChangedDocument();
    }
}
