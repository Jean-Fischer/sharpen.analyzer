using System;
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

namespace Sharpen.Analyzer.FixProvider.CSharp8;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EnableNullableContextAndDeclareIdentifierAsNullableCodeFixProvider)), Shared]
public sealed class EnableNullableContextAndDeclareIdentifierAsNullableCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.EnableNullableContextAndDeclareIdentifierAsNullableRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        // The analyzer reports on the declaring syntax location.
        // We support a conservative subset of declarations:
        // - local variables
        // - fields
        // - properties
        // - parameters
        // If we cannot confidently update the declaration, we do not offer a fix.
        if (!TryGetTypeSyntaxToMakeNullable(node, out var typeSyntax))
            return;

        // If already nullable, no fix.
        if (typeSyntax is NullableTypeSyntax)
            return;

        // Do not offer a fix for value types (the analyzer already treats them as "already nullable").
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var type = semanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
        if (type is { IsValueType: true })
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make type nullable",
                createChangedDocument: ct => MakeNullableAsync(context.Document, typeSyntax, ct),
                equivalenceKey: "MakeTypeNullable"),
            diagnostic);
    }

    private static bool TryGetTypeSyntaxToMakeNullable(SyntaxNode node, out TypeSyntax typeSyntax)
    {
        typeSyntax = null!;

        // local: string s = null;
        if (node is VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration)
            {
                typeSyntax = variableDeclaration.Type;
                return true;
            }

            return false;
        }

        // field: private string s = null;
        if (node is VariableDeclarationSyntax fieldVariableDeclaration &&
            fieldVariableDeclaration.Parent is FieldDeclarationSyntax)
        {
            typeSyntax = fieldVariableDeclaration.Type;
            return true;
        }

        // property: public string P { get; set; } = null;
        if (node is PropertyDeclarationSyntax propertyDeclaration)
        {
            typeSyntax = propertyDeclaration.Type;
            return true;
        }

        // parameter: void M(string s = null)
        if (node is ParameterSyntax parameter)
        {
            if (parameter.Type is null)
                return false;

            typeSyntax = parameter.Type;
            return true;
        }

        // Sometimes the diagnostic location can be on the identifier token; walk up.
        var ancestor = node.AncestorsAndSelf().FirstOrDefault(n =>
            n is VariableDeclaratorSyntax ||
            n is VariableDeclarationSyntax ||
            n is PropertyDeclarationSyntax ||
            n is ParameterSyntax);

        if (ancestor is null || ReferenceEquals(ancestor, node))
            return false;

        return TryGetTypeSyntaxToMakeNullable(ancestor, out typeSyntax);
    }

    private static async Task<Document> MakeNullableAsync(Document document, TypeSyntax typeSyntax, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Preserve trivia and formatting.
        var nullableType = SyntaxFactory.NullableType(typeSyntax.WithoutTrivia())
            .WithTriviaFrom(typeSyntax);

        editor.ReplaceNode(typeSyntax, nullableType);

        return editor.GetChangedDocument();
    }
}
