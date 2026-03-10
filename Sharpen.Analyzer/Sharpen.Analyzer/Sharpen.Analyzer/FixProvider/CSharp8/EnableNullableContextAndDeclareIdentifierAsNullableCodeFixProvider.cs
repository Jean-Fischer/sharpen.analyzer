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

        // The analyzer reports on the triggering node location (assignment/initializer/etc.).
        // We resolve the symbol from that node, then update the symbol's declaration type.
        var typeSyntax = await TryGetTypeSyntaxToMakeNullableAsync(context.Document, node, context.CancellationToken).ConfigureAwait(false);
        if (typeSyntax is null)
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

    private static async Task<TypeSyntax?> TryGetTypeSyntaxToMakeNullableAsync(
        Document document,
        SyntaxNode diagnosticNode,
        CancellationToken cancellationToken)
    {
        // 1) Fast path: if the diagnostic node is already a declaration (e.g. variable declarator / parameter / property)
        //    keep the old behavior.
        if (TryGetTypeSyntaxFromDeclarationNode(diagnosticNode, out var typeSyntax))
            return typeSyntax;

        // 2) Otherwise, resolve the symbol from the diagnostic node (assignment, equals, coalesce, etc.)
        //    and jump to its declaring syntax.
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return null;

        var symbol = semanticModel.GetSymbolInfo(diagnosticNode, cancellationToken).Symbol;

        // If the diagnostic node is not directly bindable (common), try to bind common sub-nodes.
        symbol ??= diagnosticNode switch
        {
            AssignmentExpressionSyntax a => semanticModel.GetSymbolInfo(a.Left, cancellationToken).Symbol,
            BinaryExpressionSyntax b when b.IsKind(SyntaxKind.EqualsExpression) || b.IsKind(SyntaxKind.NotEqualsExpression) =>
                semanticModel.GetSymbolInfo(b.Left, cancellationToken).Symbol ?? semanticModel.GetSymbolInfo(b.Right, cancellationToken).Symbol,
            BinaryExpressionSyntax b when b.IsKind(SyntaxKind.CoalesceExpression) => semanticModel.GetSymbolInfo(b.Left, cancellationToken).Symbol,
            ConditionalAccessExpressionSyntax c => semanticModel.GetSymbolInfo(c.Expression, cancellationToken).Symbol,
            _ => null
        };

        if (symbol is null)
            return null;

        var declaringSyntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
        if (declaringSyntax is null)
            return null;

        return TryGetTypeSyntaxFromDeclarationNode(declaringSyntax, out var declaredTypeSyntax)
            ? declaredTypeSyntax
            : null;
    }

    private static bool TryGetTypeSyntaxFromDeclarationNode(SyntaxNode node, out TypeSyntax typeSyntax)
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

        return TryGetTypeSyntaxFromDeclarationNode(ancestor, out typeSyntax);
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
