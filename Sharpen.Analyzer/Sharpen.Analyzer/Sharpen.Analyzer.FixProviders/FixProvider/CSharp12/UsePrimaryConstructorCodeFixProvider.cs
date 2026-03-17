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
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp12;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UsePrimaryConstructorCodeFixProvider))]
[Shared]
public sealed class UsePrimaryConstructorCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UsePrimaryConstructorRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var compilation = await context.Document.Project.GetCompilationAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (compilation is null || !CSharpLanguageVersion.IsCSharp12OrAbove(compilation))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var ctor = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
        if (ctor is null)
            return;

        var typeDecl = ctor.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use primary constructor",
                ct => UsePrimaryConstructorAsync(context.Document, typeDecl, ctor, ct),
                nameof(UsePrimaryConstructorCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> UsePrimaryConstructorAsync(
        Document document,
        TypeDeclarationSyntax typeDecl,
        ConstructorDeclarationSyntax ctor,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        // Re-acquire nodes from current root.
        var currentCtor = root.FindNode(ctor.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ConstructorDeclarationSyntax>() ?? ctor;
        var currentType = root.FindNode(typeDecl.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<TypeDeclarationSyntax>() ?? typeDecl;

        if (currentCtor.Body is null)
            return document;

        var parameters = currentCtor.ParameterList.Parameters;
        if (parameters.Count == 0)
            return document;

        // Build mapping: member symbol -> parameter syntax.
        var assignments = currentCtor.Body.Statements
            .OfType<ExpressionStatementSyntax>()
            .Select(s => s.Expression)
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.IsKind(SyntaxKind.SimpleAssignmentExpression))
            .ToArray();

        if (assignments.Length != parameters.Count)
            return document;

        var memberToParameter = assignments
            .Select(a => new
            {
                Member = semanticModel.GetSymbolInfo(a.Left, cancellationToken).Symbol,
                ParameterName = (a.Right as IdentifierNameSyntax)?.Identifier.ValueText
            })
            .Where(x => x.Member is IFieldSymbol or IPropertySymbol && x.ParameterName is not null)
            .ToDictionary(x => x.Member!, x => x.ParameterName!);

        if (memberToParameter.Count != parameters.Count)
            return document;

        // Update members: convert assigned properties/fields to get-only auto-properties initialized from parameter.
        var updatedMembers = currentType.Members;

        foreach (var kvp in memberToParameter)
        {
            var memberSymbol = kvp.Key;
            var parameterName = kvp.Value;

            var memberDecl = currentType.Members
                .FirstOrDefault(m =>
                    SymbolEqualityComparer.Default.Equals(semanticModel.GetDeclaredSymbol(m, cancellationToken),
                        memberSymbol));

            if (memberDecl is PropertyDeclarationSyntax property)
            {
                // Only handle auto-properties.
                if (property.AccessorList is null)
                    continue;

                var accessors = property.AccessorList.Accessors;
                var getAccessor = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                if (getAccessor is null)
                    continue;

                // Remove set/init accessors.
                var newAccessorList = property.AccessorList.WithAccessors(
                    SyntaxFactory.List(new[]
                    {
                        getAccessor.WithBody(null).WithExpressionBody(null)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    }));

                var newProperty = property
                    .WithAccessorList(newAccessorList)
                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(parameterName)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                updatedMembers = updatedMembers.Replace(property, newProperty);
            }
            else if (memberDecl is FieldDeclarationSyntax field)
            {
                // Only handle single-variable fields.
                if (field.Declaration.Variables.Count != 1)
                    continue;

                var variable = field.Declaration.Variables[0];
                var newVariable =
                    variable.WithInitializer(
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(parameterName)));
                var newField =
                    field.WithDeclaration(
                        field.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(newVariable)));
                updatedMembers = updatedMembers.Replace(field, newField);
            }
        }

        // Remove the constructor.
        updatedMembers = SyntaxFactory.List(updatedMembers.Where(m => !m.IsEquivalentTo(currentCtor)));

        // Add primary constructor parameter list.
        var updatedType = currentType switch
        {
            ClassDeclarationSyntax c => c.WithParameterList(SyntaxFactory.ParameterList(parameters)),
            StructDeclarationSyntax s => s.WithParameterList(SyntaxFactory.ParameterList(parameters)),
            _ => currentType
        };

        updatedType = updatedType.WithMembers(updatedMembers);

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(currentType, updatedType);
        return editor.GetChangedDocument();
    }
}