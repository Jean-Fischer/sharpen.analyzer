using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExtensionBlocksCodeFixProvider))]
[Shared]
public sealed class UseExtensionBlocksCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseExtensionBlocksRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>() ?? node as ClassDeclarationSyntax;
        if (classDeclaration is null)
            return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new ExtensionBlocksSafetyChecker(),
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
            CSharp14Rules.UseExtensionBlocksRule.Title.ToString(),
            nameof(UseExtensionBlocksCodeFixProvider),
            ct => ApplyAsync(context.Document, classDeclaration, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, ClassDeclarationSyntax classDeclaration,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var currentClass = root.FindNode(classDeclaration.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (currentClass is null)
            return document;

        var extensionMethods = currentClass.Members
            .Where(m => m is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .Where(m => m.ParameterList.Parameters.Any())
            .Where(m => m.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
            .ToList();

        if (extensionMethods.Count < 2)
            return document;

        // Pick dominant receiver type by syntax string.
        var dominantGroup = extensionMethods
            .GroupBy(m => m.ParameterList.Parameters[0].Type?.ToString() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (dominantGroup is null || dominantGroup.Count() < 2)
            return document;

        var receiverTypeSyntax = dominantGroup.First().ParameterList.Parameters[0].Type;
        if (receiverTypeSyntax is null)
            return document;

        // Create: extension <ReceiverType> { ...methods... }
        // NOTE: Roslyn may not have dedicated syntax nodes yet; we use ParseMemberDeclaration.
        var methodsText = string.Join("\n\n",
            dominantGroup.Select(m => m.WithLeadingTrivia().WithTrailingTrivia().ToFullString()));
        var extensionBlockText = $"extension {receiverTypeSyntax}\n{{\n{methodsText}\n}}";

        var parsed = SyntaxFactory.ParseMemberDeclaration(extensionBlockText);
        if (parsed is null)
            return document;

        parsed = parsed
            .WithLeadingTrivia(dominantGroup.First().GetLeadingTrivia())
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Re-find nodes in the editor's current root to avoid GetCurrentNode() failures.
        var editorRoot = editor.OriginalRoot;
        var editorClass = editorRoot.FindNode(currentClass.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (editorClass is null)
            return document;

        var editorMethods = editorClass.Members
            .Where(m => m is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .Where(m => m.ParameterList.Parameters.Any())
            .Where(m => m.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
            .ToList();

        var editorDominantGroup = editorMethods
            .GroupBy(m => m.ParameterList.Parameters[0].Type?.ToString() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (editorDominantGroup is null || editorDominantGroup.Count() < 2)
            return document;

        // Insert extension block at the position of the first moved method.
        editor.InsertBefore(editorDominantGroup.First(), parsed);

        // Remove moved methods.
        foreach (var method in editorDominantGroup)
            editor.RemoveNode(method, SyntaxRemoveOptions.KeepExteriorTrivia);

        return editor.GetChangedDocument();
    }
}