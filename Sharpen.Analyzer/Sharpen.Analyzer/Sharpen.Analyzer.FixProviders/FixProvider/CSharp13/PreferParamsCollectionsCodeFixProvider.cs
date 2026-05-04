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
using Microsoft.CodeAnalysis.FindSymbols;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;
using Sharpen.Analyzer.Safety.FixProviderSafety;

namespace Sharpen.Analyzer.FixProvider.CSharp13;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferParamsCollectionsCodeFixProvider))]
[Shared]
public sealed class PreferParamsCollectionsCodeFixProvider
    : CSharp13OrAboveSafetyCheckedSharpenCodeFixProvider<ParameterSyntax, PreferParamsCollectionsSafetyChecker>
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.PreferParamsCollectionsRule.Id);

    protected override ParameterSyntax? TryGetTargetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ParameterSyntax>();
    }

    protected override Task RegisterSafetyCheckedCodeFixesAsync(
        CodeFixContext context,
        SyntaxNode root,
        Diagnostic diagnostic,
        ParameterSyntax targetNode)
    {
        RegisterCodeFix(
            context,
            diagnostic,
            "Prefer collection-based params",
            nameof(PreferParamsCollectionsCodeFixProvider),
            ct => ApplyAsync(context.Document, targetNode, ct));

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyAsync(Document document, ParameterSyntax parameter, CancellationToken ct)
    {
        var rewriteSymbols = await TryGetRewriteSymbolsAsync(document, parameter, ct).ConfigureAwait(false);
        if (rewriteSymbols is null)
            return document;

        var updatedDocument = await UpdateDeclarationAsync(document, parameter, rewriteSymbols.NewParamType, ct).ConfigureAwait(false);
        return await UpdateExpandedCallSitesAsync(
            updatedDocument,
            rewriteSymbols.MethodSymbol,
            rewriteSymbols.ParameterSymbol,
            rewriteSymbols.ElementType,
            ct).ConfigureAwait(false);
    }

    private static async Task<ParamsRewriteSymbols?> TryGetRewriteSymbolsAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return null;

        var method = parameter.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
        if (method is null)
            return null;

        var methodSymbol = semanticModel.GetDeclaredSymbol(method, cancellationToken);
        var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken);
        if (methodSymbol is null || parameterSymbol is null)
            return null;

        var elementType = (parameterSymbol.Type as IArrayTypeSymbol)?.ElementType;
        if (elementType is null)
            return null;

        var readOnlySpanType = semanticModel.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
        if (readOnlySpanType is null)
            return null;

        return new ParamsRewriteSymbols(
            methodSymbol,
            parameterSymbol,
            elementType,
            readOnlySpanType.Construct(elementType));
    }

    private static async Task<Document> UpdateDeclarationAsync(
        Document document,
        ParameterSyntax parameter,
        INamedTypeSymbol newParamType,
        CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var newTypeSyntax = SyntaxFactory
            .ParseTypeName(newParamType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .WithTriviaFrom(parameter.Type!);

        editor.ReplaceNode(parameter.Type!, newTypeSyntax);
        return editor.GetChangedDocument();
    }

    private static async Task<Document> UpdateExpandedCallSitesAsync(
        Document updatedDocument,
        IMethodSymbol methodSymbol,
        IParameterSymbol parameterSymbol,
        ITypeSymbol elementType,
        CancellationToken cancellationToken)
    {
        var updatedSolution = updatedDocument.Project.Solution;
        var references = await SymbolFinder.FindReferencesAsync(methodSymbol, updatedSolution, cancellationToken)
            .ConfigureAwait(false);

        foreach (var reference in references)
        {
            foreach (var location in reference.Locations)
            {
                updatedSolution = await UpdateReferenceLocationAsync(
                        updatedSolution,
                        location,
                        methodSymbol,
                        parameterSymbol,
                        elementType,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return updatedSolution.GetDocument(updatedDocument.Id) ?? updatedDocument;
    }

    private static async Task<Solution> UpdateReferenceLocationAsync(
        Solution solution,
        ReferenceLocation location,
        IMethodSymbol methodSymbol,
        IParameterSymbol parameterSymbol,
        ITypeSymbol elementType,
        CancellationToken cancellationToken)
    {
        var refDocument = solution.GetDocument(location.Document.Id);
        if (refDocument is null)
            return solution;

        var refRoot = await refDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var refSemanticModel = await refDocument.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (refRoot is null || refSemanticModel is null)
            return solution;

        var node = refRoot.FindNode(location.Location.SourceSpan, getInnermostNodeForTie: true);
        var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (!IsMatchingInvocation(invocation, refSemanticModel, methodSymbol, cancellationToken))
            return solution;

        var rewrittenInvocation = TryRewriteExpandedInvocation(invocation!, methodSymbol, parameterSymbol, elementType);
        if (rewrittenInvocation is null)
            return solution;

        var editor = await DocumentEditor.CreateAsync(refDocument, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(invocation!, rewrittenInvocation);
        return editor.GetChangedDocument().Project.Solution;
    }

    private static bool IsMatchingInvocation(
        InvocationExpressionSyntax? invocation,
        SemanticModel semanticModel,
        IMethodSymbol methodSymbol,
        CancellationToken cancellationToken)
    {
        if (invocation is null)
            return false;

        return semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol invokedSymbol
               && SymbolEqualityComparer.Default.Equals(invokedSymbol.OriginalDefinition, methodSymbol.OriginalDefinition);
    }

    private static InvocationExpressionSyntax? TryRewriteExpandedInvocation(
        InvocationExpressionSyntax invocation,
        IMethodSymbol methodSymbol,
        IParameterSymbol parameterSymbol,
        ITypeSymbol elementType)
    {
        var args = invocation.ArgumentList.Arguments;
        var paramIndex = parameterSymbol.Ordinal;
        if (args.Count <= paramIndex || args.Count <= methodSymbol.Parameters.Length)
            return null;

        var arrayCreation = CreateExpandedParamsArray(args.Skip(paramIndex).ToImmutableArray(), elementType);
        var newArgs = args.Take(paramIndex)
            .Concat(new[] { SyntaxFactory.Argument(arrayCreation).WithTriviaFrom(args[paramIndex]) })
            .ToImmutableArray();

        return invocation.WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(newArgs)));
    }

    private static ArrayCreationExpressionSyntax CreateExpandedParamsArray(
        ImmutableArray<ArgumentSyntax> expandedArgs,
        ITypeSymbol elementType)
    {
        return SyntaxFactory.ArrayCreationExpression(
                SyntaxFactory.ArrayType(
                    SyntaxFactory.ParseTypeName(elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
                    SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))))
            .WithInitializer(SyntaxFactory.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                SyntaxFactory.SeparatedList(expandedArgs.Select(a => a.Expression))));
    }

    private sealed class ParamsRewriteSymbols(
        IMethodSymbol methodSymbol,
        IParameterSymbol parameterSymbol,
        ITypeSymbol elementType,
        INamedTypeSymbol newParamType)
    {
        public IMethodSymbol MethodSymbol { get; } = methodSymbol;
        public IParameterSymbol ParameterSymbol { get; } = parameterSymbol;
        public ITypeSymbol ElementType { get; } = elementType;
        public INamedTypeSymbol NewParamType { get; } = newParamType;
    }
}
