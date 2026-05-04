using System.Collections.Generic;
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

        var diagnostic = context.Diagnostics[0];
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

        var currentCtor = GetCurrentConstructor(root, ctor) ?? ctor;
        var currentType = GetCurrentType(root, typeDecl) ?? typeDecl;
        if (currentCtor.Body is null || !currentCtor.ParameterList.Parameters.Any())
            return document;

        if (!TryCreateMemberToParameterMap(semanticModel, currentCtor, cancellationToken, out var memberToParameter))
            return document;

        var updatedType = UpdateTypeDeclaration(currentType, currentCtor, semanticModel, memberToParameter, cancellationToken);

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(currentType, updatedType);
        return editor.GetChangedDocument();
    }

    private static ConstructorDeclarationSyntax? GetCurrentConstructor(SyntaxNode root, ConstructorDeclarationSyntax ctor)
    {
        return root.FindNode(ctor.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
    }

    private static TypeDeclarationSyntax? GetCurrentType(SyntaxNode root, TypeDeclarationSyntax typeDecl)
    {
        return root.FindNode(typeDecl.Span, getInnermostNodeForTie: true).FirstAncestorOrSelf<TypeDeclarationSyntax>();
    }

    private static bool TryCreateMemberToParameterMap(
        SemanticModel semanticModel,
        ConstructorDeclarationSyntax ctor,
        CancellationToken cancellationToken,
        out Dictionary<ISymbol, string> memberToParameter)
    {
        memberToParameter = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);

        var parameters = ctor.ParameterList.Parameters;
        var assignments = ctor.Body!.Statements
            .OfType<ExpressionStatementSyntax>()
            .Select(s => s.Expression)
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.IsKind(SyntaxKind.SimpleAssignmentExpression))
            .ToArray();

        if (assignments.Length != parameters.Count)
            return false;

        foreach (var assignment in assignments)
        {
            var member = semanticModel.GetSymbolInfo(assignment.Left, cancellationToken).Symbol;
            var parameterName = (assignment.Right as IdentifierNameSyntax)?.Identifier.ValueText;
            if (member is not IFieldSymbol and not IPropertySymbol || parameterName is null)
                return false;

            memberToParameter[member] = parameterName;
        }

        return memberToParameter.Count == parameters.Count;
    }

    private static TypeDeclarationSyntax UpdateTypeDeclaration(
        TypeDeclarationSyntax currentType,
        ConstructorDeclarationSyntax currentCtor,
        SemanticModel semanticModel,
        IReadOnlyDictionary<ISymbol, string> memberToParameter,
        CancellationToken cancellationToken)
    {
        var updatedMembers = UpdateMembers(currentType, semanticModel, memberToParameter, cancellationToken);
        updatedMembers = SyntaxFactory.List(updatedMembers.Where(m => !m.IsEquivalentTo(currentCtor)));

        return AddPrimaryConstructorParameterList(currentType, currentCtor.ParameterList.Parameters)
            .WithMembers(updatedMembers);
    }

    private static SyntaxList<MemberDeclarationSyntax> UpdateMembers(
        TypeDeclarationSyntax currentType,
        SemanticModel semanticModel,
        IReadOnlyDictionary<ISymbol, string> memberToParameter,
        CancellationToken cancellationToken)
    {
        var updatedMembers = currentType.Members;

        foreach (var kvp in memberToParameter)
        {
            var memberDecl = FindMemberDeclaration(currentType, semanticModel, kvp.Key, cancellationToken);
            var updatedMember = RewriteAssignedMember(memberDecl, kvp.Value);
            if (memberDecl != null && updatedMember != null)
                updatedMembers = updatedMembers.Replace(memberDecl, updatedMember);
        }

        return updatedMembers;
    }

    private static MemberDeclarationSyntax? FindMemberDeclaration(
        TypeDeclarationSyntax currentType,
        SemanticModel semanticModel,
        ISymbol memberSymbol,
        CancellationToken cancellationToken)
    {
        return currentType.Members.FirstOrDefault(member =>
            SymbolEqualityComparer.Default.Equals(semanticModel.GetDeclaredSymbol(member, cancellationToken), memberSymbol));
    }

    private static MemberDeclarationSyntax? RewriteAssignedMember(MemberDeclarationSyntax? memberDecl, string parameterName)
    {
        return memberDecl switch
        {
            PropertyDeclarationSyntax property => RewriteAssignedProperty(property, parameterName),
            FieldDeclarationSyntax field => RewriteAssignedField(field, parameterName),
            _ => memberDecl
        };
    }

    private static PropertyDeclarationSyntax? RewriteAssignedProperty(PropertyDeclarationSyntax property, string parameterName)
    {
        if (property.AccessorList is null)
            return null;

        var getAccessor = property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getAccessor is null)
            return null;

        var newAccessorList = property.AccessorList.WithAccessors(
            SyntaxFactory.List(new[]
            {
                getAccessor.WithBody(null).WithExpressionBody(null)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            }));

        return property
            .WithAccessorList(newAccessorList)
            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(parameterName)))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private static FieldDeclarationSyntax? RewriteAssignedField(FieldDeclarationSyntax field, string parameterName)
    {
        if (field.Declaration.Variables.Count != 1)
            return null;

        var variable = field.Declaration.Variables[0].WithInitializer(
            SyntaxFactory.EqualsValueClause(SyntaxFactory.IdentifierName(parameterName)));
        return field.WithDeclaration(
            field.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(variable)));
    }

    private static TypeDeclarationSyntax AddPrimaryConstructorParameterList(
        TypeDeclarationSyntax typeDeclaration,
        SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax c => c.WithParameterList(SyntaxFactory.ParameterList(parameters)),
            StructDeclarationSyntax s => s.WithParameterList(SyntaxFactory.ParameterList(parameters)),
            _ => typeDeclaration
        };
    }
}
