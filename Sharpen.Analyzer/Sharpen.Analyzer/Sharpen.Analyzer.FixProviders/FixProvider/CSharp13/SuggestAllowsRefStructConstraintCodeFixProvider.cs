using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProviders.FixProvider.CSharp13;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuggestAllowsRefStructConstraintCodeFixProvider))]
[Shared]
public sealed class SuggestAllowsRefStructConstraintCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.SuggestAllowsRefStructConstraintRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add 'allows ref struct' constraint (requires review)",
                    ct => AddAllowsRefStructConstraintAsync(context.Document, method, ct),
                    "AddAllowsRefStructConstraint"),
                diagnostic);
            return;
        }

        var typeDecl = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is not null)
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add 'allows ref struct' constraint (requires review)",
                    ct => AddAllowsRefStructConstraintAsync(context.Document, typeDecl, ct),
                    "AddAllowsRefStructConstraint"),
                diagnostic);
    }

    private static async Task<Document> AddAllowsRefStructConstraintAsync(Document document,
        MethodDeclarationSyntax method, CancellationToken ct)
    {
        if (method.TypeParameterList is null || method.TypeParameterList.Parameters.Count == 0)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        var typeParamName = method.TypeParameterList.Parameters[0].Identifier.ValueText;
        var clause = CreateAllowsRefStructConstraintClause(typeParamName);

        var newMethod = method.AddConstraintClauses(clause)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(method, newMethod);

        return editor.GetChangedDocument();
    }

    private static async Task<Document> AddAllowsRefStructConstraintAsync(Document document,
        TypeDeclarationSyntax typeDecl, CancellationToken ct)
    {
        if (typeDecl.TypeParameterList is null || typeDecl.TypeParameterList.Parameters.Count == 0)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        var typeParamName = typeDecl.TypeParameterList.Parameters[0].Identifier.ValueText;
        var clause = CreateAllowsRefStructConstraintClause(typeParamName);

        var newType = typeDecl.AddConstraintClauses(clause)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(typeDecl, newType);

        return editor.GetChangedDocument();
    }

    private static TypeParameterConstraintClauseSyntax CreateAllowsRefStructConstraintClause(string typeParameterName)
    {
        // Syntax: where T : allows ref struct
        // Note: Roslyn doesn't expose a dedicated factory for this new constraint yet,
        // so we parse a full clause and extract it.
        var method = (MethodDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(
            $"void M<{typeParameterName}>() where {typeParameterName} : allows ref struct {{ }}")!;

        return method.ConstraintClauses[0]
            .WithAdditionalAnnotations(Formatter.Annotation);
    }
}