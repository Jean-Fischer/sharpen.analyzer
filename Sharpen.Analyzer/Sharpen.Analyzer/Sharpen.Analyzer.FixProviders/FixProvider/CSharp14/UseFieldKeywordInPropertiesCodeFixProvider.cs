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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseFieldKeywordInPropertiesCodeFixProvider))]
[Shared]
public sealed class UseFieldKeywordInPropertiesCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp14Rules.UseFieldKeywordInPropertiesRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var property = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();
        if (property is null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            checker: new FieldBackedPropertySafetyChecker(),
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
            title: "Use field-backed property",
            equivalenceKey: nameof(UseFieldKeywordInPropertiesCodeFixProvider),
            createChangedDocument: ct => ApplyAsync(context.Document, property, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, PropertyDeclarationSyntax property, CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Re-find the property in the current root (the node passed in may be from a different snapshot).
        var currentProperty = root.FindNode(property.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
        if (currentProperty?.AccessorList is null)
            return document;

        var getAccessor = currentProperty.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        var setAccessor = currentProperty.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (getAccessor is null || setAccessor is null)
            return document;

        if (!TryGetBackingFieldFromGetter(semanticModel, getAccessor, ct, out var backingFieldSymbol) || backingFieldSymbol is null)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Remove the backing field declaration.
        var fieldDecl = root
            .DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Where(v => v.Identifier.ValueText == backingFieldSymbol.Name)
            .Select(v => v.FirstAncestorOrSelf<FieldDeclarationSyntax>())
            .FirstOrDefault(f => f is not null);

        if (fieldDecl is not null)
        {
            // If the field declaration contains multiple variables, only remove the matching variable.
            if (fieldDecl.Declaration.Variables.Count > 1)
            {
                var newVariables = fieldDecl.Declaration.Variables
                    .Where(v => v.Identifier.ValueText != backingFieldSymbol.Name)
                    .ToList();

                var newDeclaration = fieldDecl.Declaration.WithVariables(SyntaxFactory.SeparatedList(newVariables));
                editor.ReplaceNode(fieldDecl, fieldDecl.WithDeclaration(newDeclaration));
            }
            else
            {
                editor.RemoveNode(fieldDecl, SyntaxRemoveOptions.KeepExteriorTrivia);
            }
        }

        // Rewrite accessor bodies to use `field`.
        var newGetAccessor = ReplaceFieldIdentifierInAccessor(getAccessor, backingFieldSymbol.Name);
        var newSetAccessor = ReplaceFieldIdentifierInAccessor(setAccessor, backingFieldSymbol.Name);

        var newAccessorList = currentProperty.AccessorList
            .ReplaceNode(getAccessor, newGetAccessor)
            .ReplaceNode(setAccessor, newSetAccessor);

        editor.ReplaceNode(currentProperty, currentProperty.WithAccessorList(newAccessorList));

        return editor.GetChangedDocument();
    }



    private static AccessorDeclarationSyntax ReplaceFieldIdentifierInAccessor(AccessorDeclarationSyntax accessor, string fieldName)
    {
        // Prefer a targeted rewrite for the supported patterns.
        if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
            return RewriteGetter(accessor, fieldName);

        if (accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            return RewriteSetter(accessor, fieldName);

        return accessor;
    }

    private static AccessorDeclarationSyntax RewriteGetter(AccessorDeclarationSyntax getAccessor, string fieldName)
    {
        if (getAccessor.ExpressionBody is not null)
        {
            var expr = getAccessor.ExpressionBody.Expression;
            if (expr is IdentifierNameSyntax id && id.Identifier.ValueText == fieldName)
            {
                var newExprBody = getAccessor.ExpressionBody.WithExpression(
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("field")).WithTriviaFrom(id));
                return getAccessor.WithExpressionBody(newExprBody);
            }

            return getAccessor;
        }

        if (getAccessor.Body is not null)
        {
            if (getAccessor.Body.Statements.Count == 1 &&
                getAccessor.Body.Statements[0] is ReturnStatementSyntax { Expression: IdentifierNameSyntax id } &&
                id.Identifier.ValueText == fieldName)
            {
                var newReturn = ((ReturnStatementSyntax)getAccessor.Body.Statements[0])
                    .WithExpression(SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("field")).WithTriviaFrom(id));

                return getAccessor.WithBody(getAccessor.Body.WithStatements(SyntaxFactory.SingletonList<StatementSyntax>(newReturn)));
            }

            return getAccessor;
        }

        return getAccessor;
    }

    private static AccessorDeclarationSyntax RewriteSetter(AccessorDeclarationSyntax setAccessor, string fieldName)
    {
        if (setAccessor.ExpressionBody is not null)
        {
            if (setAccessor.ExpressionBody.Expression is AssignmentExpressionSyntax assignment &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignment.Left is IdentifierNameSyntax leftId &&
                leftId.Identifier.ValueText == fieldName)
            {
                var newAssignment = assignment.WithLeft(
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("field")).WithTriviaFrom(leftId));
                return setAccessor.WithExpressionBody(setAccessor.ExpressionBody.WithExpression(newAssignment));
            }

            return setAccessor;
        }

        if (setAccessor.Body is not null)
        {
            if (setAccessor.Body.Statements.Count == 1 &&
                setAccessor.Body.Statements[0] is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } &&
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignment.Left is IdentifierNameSyntax leftId &&
                leftId.Identifier.ValueText == fieldName)
            {
                var newAssignment = assignment.WithLeft(
                    SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("field")).WithTriviaFrom(leftId));
                var newStatement = ((ExpressionStatementSyntax)setAccessor.Body.Statements[0]).WithExpression(newAssignment);
                return setAccessor.WithBody(setAccessor.Body.WithStatements(SyntaxFactory.SingletonList<StatementSyntax>(newStatement)));
            }

            return setAccessor;
        }

        return setAccessor;
    }


    private static bool TryGetBackingFieldFromGetter(
        SemanticModel semanticModel,
        AccessorDeclarationSyntax getAccessor,
        CancellationToken ct,
        out IFieldSymbol? backingFieldSymbol)
    {
        backingFieldSymbol = null;

        ExpressionSyntax? returnedExpression = null;

        if (getAccessor.ExpressionBody is not null)
        {
            returnedExpression = getAccessor.ExpressionBody.Expression;
        }
        else if (getAccessor.Body is not null)
        {
            var statements = getAccessor.Body.Statements;
            if (statements.Count != 1)
                return false;

            if (statements[0] is not ReturnStatementSyntax returnStatement)
                return false;

            returnedExpression = returnStatement.Expression;
        }

        if (returnedExpression is null)
            return false;

        var symbol = semanticModel.GetSymbolInfo(returnedExpression, ct).Symbol;
        if (symbol is not IFieldSymbol fieldSymbol)
            return false;

        backingFieldSymbol = fieldSymbol;
        return true;
    }
}
