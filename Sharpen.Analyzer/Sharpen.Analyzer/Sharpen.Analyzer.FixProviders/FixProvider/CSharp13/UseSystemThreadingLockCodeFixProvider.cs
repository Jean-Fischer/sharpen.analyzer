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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseSystemThreadingLockCodeFixProvider))]
[Shared]
public sealed class UseSystemThreadingLockCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.UseSystemThreadingLockRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var variable = node.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        if (variable is null)
            return;

        var semanticModel =
            await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            new UseSystemThreadingLockSafetyChecker(),
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
            "Use System.Threading.Lock",
            nameof(UseSystemThreadingLockCodeFixProvider),
            ct => ApplyAsync(context.Document, variable, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, VariableDeclaratorSyntax variable,
        CancellationToken ct)
    {
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        var fieldDeclaration = variable.FirstAncestorOrSelf<FieldDeclarationSyntax>();
        if (fieldDeclaration is null)
            return document;

        var variableDeclaration = fieldDeclaration.Declaration;

        // Replace the field declaration type with System.Threading.Lock.
        var newType = SyntaxFactory.ParseTypeName("System.Threading.Lock")
            .WithTriviaFrom(variableDeclaration.Type);

        // Ensure initializer is `new()`.
        var newInitializer = SyntaxFactory.EqualsValueClause(SyntaxFactory.ImplicitObjectCreationExpression());
        if (variable.Initializer is not null)
            newInitializer = newInitializer.WithTriviaFrom(variable.Initializer);

        var newVariable = variable.WithInitializer(newInitializer);

        var newVariableDeclaration = variableDeclaration
            .WithType(newType)
            .ReplaceNode(variable, newVariable);

        var newFieldDeclaration = fieldDeclaration.WithDeclaration(newVariableDeclaration);
        editor.ReplaceNode(fieldDeclaration, newFieldDeclaration);

        return editor.GetChangedDocument();
    }
}