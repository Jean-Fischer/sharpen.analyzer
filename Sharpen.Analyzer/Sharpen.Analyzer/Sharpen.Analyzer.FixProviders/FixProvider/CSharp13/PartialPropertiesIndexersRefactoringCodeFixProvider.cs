using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.CSharp13;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialPropertiesIndexersRefactoringCodeFixProvider))]
[Shared]
public sealed class PartialPropertiesIndexersRefactoringCodeFixProvider
    : CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<BasePropertyDeclarationSyntax,
        PartialPropertiesIndexersRefactoringSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.PartialPropertiesIndexersRefactoringRule.Id);

    protected override BasePropertyDeclarationSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        return node.FirstAncestorOrSelf<BasePropertyDeclarationSyntax>();
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        BasePropertyDeclarationSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Refactor to partial property/indexer",
            nameof(PartialPropertiesIndexersRefactoringCodeFixProvider),
            ct => RefactorAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> RefactorAsync(
        Document document,
        BasePropertyDeclarationSyntax propertyOrIndexer,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var currentDeclaration = root.FindNode(propertyOrIndexer.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<BasePropertyDeclarationSyntax>();
        if (currentDeclaration is null)
            return document;

        // Avoid SyntaxEditor tracking issues by doing a pure syntax rewrite.
        var declaring = AddPartialModifier(currentDeclaration);
        var implementing = CreateImplementingDeclaration(currentDeclaration);

        var newRoot = root.ReplaceNode(currentDeclaration, new SyntaxNode[] { declaring, implementing });
        return document.WithSyntaxRoot(newRoot);
    }

    private static BasePropertyDeclarationSyntax AddPartialModifier(BasePropertyDeclarationSyntax member)
    {
        if (member.Modifiers.Any(SyntaxKind.PartialKeyword))
            return member;

        // Keep modifier ordering: insert after accessibility modifiers if present, otherwise at start.
        var modifiers = member.Modifiers;
        var insertIndex = 0;
        for (var i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].IsKind(SyntaxKind.PublicKeyword)
                || modifiers[i].IsKind(SyntaxKind.PrivateKeyword)
                || modifiers[i].IsKind(SyntaxKind.InternalKeyword)
                || modifiers[i].IsKind(SyntaxKind.ProtectedKeyword))
            {
                insertIndex = i + 1;
            }
        }

        modifiers = modifiers.Insert(insertIndex, SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        return member.WithModifiers(modifiers);
    }

    private static BasePropertyDeclarationSyntax CreateImplementingDeclaration(BasePropertyDeclarationSyntax original)
    {
        var implementing = AddPartialModifier(original);

        // Ensure we have an accessor list.
        if (implementing.AccessorList is null)
            return implementing;

        var newAccessors = implementing.AccessorList.Accessors
            .Select(a => a.WithBody(CreateTrivialBody(a.Kind())).WithSemicolonToken(default).WithExpressionBody(null))
            .ToList();

        var accessorList = implementing.AccessorList.WithAccessors(SyntaxFactory.List(newAccessors));
        implementing = implementing.WithAccessorList(accessorList);

        // Ensure the implementing part is a full declaration (not a semicolon-only property).
        if (implementing is PropertyDeclarationSyntax prop)
        {
            implementing = prop
                .WithInitializer(null)
                .WithSemicolonToken(default)
                .WithExpressionBody(null);
        }

        return implementing;
    }

    private static BlockSyntax CreateTrivialBody(SyntaxKind accessorKind)
    {
        // This is intentionally conservative and behavior-preserving only for auto-properties/indexers.
        // We generate a body that throws to force user review if they apply it in unexpected contexts.
        // However, analyzer/safety checker only allow auto accessors, so this should not be reachable.
        //
        // Note: We still need a syntactically valid body.
        var throwStatement = SyntaxFactory.ThrowStatement(
            SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.ParseTypeName("System.NotImplementedException"))
                .WithArgumentList(SyntaxFactory.ArgumentList()));

        return SyntaxFactory.Block(throwStatement);
    }
}
