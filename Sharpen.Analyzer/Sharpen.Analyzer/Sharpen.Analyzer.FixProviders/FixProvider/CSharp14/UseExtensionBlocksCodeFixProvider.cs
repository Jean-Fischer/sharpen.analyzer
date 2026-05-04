using System;
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
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.CSharp14;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseExtensionBlocksCodeFixProvider))]
[Shared]
public sealed class UseExtensionBlocksCodeFixProvider
    : SafetyCheckedSharpenCodeFixProvider<ClassDeclarationSyntax, ExtensionBlocksSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseExtensionBlocksRule.Id);

    protected override ClassDeclarationSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node.FirstAncestorOrSelf<ClassDeclarationSyntax>() ?? node as ClassDeclarationSyntax;
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        ClassDeclarationSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            CSharp14Rules.UseExtensionBlocksRule.Title.ToString(),
            nameof(UseExtensionBlocksCodeFixProvider),
            ct => ApplyAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
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

        var editorReceiverTypeSyntax = editorDominantGroup.First().ParameterList.Parameters[0].Type;
        if (editorReceiverTypeSyntax is null)
            return document;

        var receiverParameter = editorDominantGroup.First().ParameterList.Parameters[0];
        var receiverParameterName = receiverParameter.Identifier.ValueText;
        if (editorDominantGroup.Select(method => method.ParameterList.Parameters[0].Identifier.ValueText).Distinct().Count() != 1)
            return document;

        var methodsText = string.Join(
            Environment.NewLine + Environment.NewLine,
            editorDominantGroup
                .Select(static method => ConvertToExtensionBlockMethod(method).NormalizeWhitespace().ToFullString()));

        var extensionBlockText =
            $"extension({editorReceiverTypeSyntax.NormalizeWhitespace().ToFullString()} {receiverParameterName}){Environment.NewLine}{{{Environment.NewLine}{methodsText}{Environment.NewLine}}}";
        var extensionBlock = SyntaxFactory.ParseMemberDeclaration(extensionBlockText);
        if (extensionBlock is null)
            return document;

        extensionBlock = extensionBlock
            .WithLeadingTrivia(editorDominantGroup.First().GetLeadingTrivia())
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Insert extension block at the position of the first moved method.
        editor.InsertBefore(editorDominantGroup.First(), extensionBlock);

        // Remove moved methods.
        foreach (var method in editorDominantGroup)
            editor.RemoveNode(method, SyntaxRemoveOptions.KeepExteriorTrivia);

        return await Formatter.FormatAsync(editor.GetChangedDocument(), cancellationToken: ct).ConfigureAwait(false);
    }

    private static MethodDeclarationSyntax ConvertToExtensionBlockMethod(MethodDeclarationSyntax method)
    {
        var remainingParameters = method.ParameterList.Parameters.RemoveAt(0);
        var updatedModifiers = new SyntaxTokenList(method.Modifiers.Where(static modifier => !modifier.IsKind(SyntaxKind.StaticKeyword)));

        return method
            .WithModifiers(updatedModifiers)
            .WithParameterList(method.ParameterList.WithParameters(remainingParameters));
    }
}
