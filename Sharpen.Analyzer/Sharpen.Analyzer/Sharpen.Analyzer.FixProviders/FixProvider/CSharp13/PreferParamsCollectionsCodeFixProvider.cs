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

namespace Sharpen.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PreferParamsCollectionsCodeFixProvider))]
[Shared]
public sealed class PreferParamsCollectionsCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.PreferParamsCollectionsRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var parameter = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ParameterSyntax>();
        if (parameter is null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (semanticModel is null)
            return;

        // Fix-provider-side safety gate: only offer code actions when the mapped safety checker says it's safe.
        var safetyEvaluation = FixProviderSafetyRunner.EvaluateOrMatchFailed(
            checker: new PreferParamsCollectionsSafetyChecker(),
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
            title: "Prefer collection-based params",
            equivalenceKey: nameof(PreferParamsCollectionsCodeFixProvider),
            createChangedDocument: ct => ApplyAsync(context.Document, parameter, ct));
    }

    private static async Task<Document> ApplyAsync(Document document, ParameterSyntax parameter, CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var method = parameter.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>();
        if (method is null)
            return document;

        var methodSymbol = semanticModel.GetDeclaredSymbol(method, ct) as IMethodSymbol;
        if (methodSymbol is null)
            return document;

        var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, ct) as IParameterSymbol;
        if (parameterSymbol is null)
            return document;

        // Target type: ReadOnlySpan<T>
        var elementType = (parameterSymbol.Type as IArrayTypeSymbol)?.ElementType;
        if (elementType is null)
            return document;

        var readOnlySpanType = semanticModel.Compilation.GetTypeByMetadataName("System.ReadOnlySpan`1");
        if (readOnlySpanType is null)
            return document;

        var newParamType = readOnlySpanType.Construct(elementType);

        // 1) Update declaration
        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
        var newTypeSyntax = SyntaxFactory.ParseTypeName(newParamType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
            .WithTriviaFrom(parameter.Type!);

        editor.ReplaceNode(parameter.Type!, newTypeSyntax);

        var updatedDocument = editor.GetChangedDocument();
        var updatedSolution = updatedDocument.Project.Solution;

        // 2) Update in-solution call sites
        var updatedCompilation = await updatedDocument.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        if (updatedCompilation is null)
            return updatedDocument;

        // We keep using the original symbol for reference search; SymbolFinder works across the solution.
        var references = await SymbolFinder.FindReferencesAsync(methodSymbol, updatedSolution, ct).ConfigureAwait(false);

        foreach (var reference in references)
        {
            foreach (var location in reference.Locations)
            {
                var refDocument = updatedSolution.GetDocument(location.Document.Id);
                if (refDocument is null)
                    continue;

                var refRoot = await refDocument.GetSyntaxRootAsync(ct).ConfigureAwait(false);
                if (refRoot is null)
                    continue;

                var node = refRoot.FindNode(location.Location.SourceSpan, getInnermostNodeForTie: true);
                var invocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
                if (invocation is null)
                    continue;

                var refSemanticModel = await refDocument.GetSemanticModelAsync(ct).ConfigureAwait(false);
                if (refSemanticModel is null)
                    continue;

                var invokedSymbol = refSemanticModel.GetSymbolInfo(invocation, ct).Symbol as IMethodSymbol;
                if (invokedSymbol is null)
                    continue;

                // Only update invocations that bind to the same method.
                if (!SymbolEqualityComparer.Default.Equals(invokedSymbol.OriginalDefinition, methodSymbol.OriginalDefinition))
                    continue;

                // If the call already passes an array explicitly, leave it (ReadOnlySpan<T> can be created from array implicitly).
                // If the call uses expanded params arguments, wrap them into an array creation.
                var args = invocation.ArgumentList.Arguments;
                var paramIndex = parameterSymbol.Ordinal;
                if (args.Count <= paramIndex)
                    continue;

                // Determine if this is an expanded params call: more args than parameters.
                // (If the call passes a single argument at the params position, we assume it's already an array-like value.)
                var isExpanded = args.Count > methodSymbol.Parameters.Length;
                if (!isExpanded)
                    continue;

                var expandedArgs = args.Skip(paramIndex).ToImmutableArray();
                var arrayCreation = SyntaxFactory.ArrayCreationExpression(
                        SyntaxFactory.ArrayType(
                            SyntaxFactory.ParseTypeName(elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
                            SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())))))
                    .WithInitializer(SyntaxFactory.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        SyntaxFactory.SeparatedList(expandedArgs.Select(a => a.Expression))));

                var newArgs = args.Take(paramIndex)
                    .Concat(new[] { SyntaxFactory.Argument(arrayCreation).WithTriviaFrom(args[paramIndex]) })
                    .ToImmutableArray();

                var newInvocation = invocation.WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(newArgs)));

                var refEditor = await DocumentEditor.CreateAsync(refDocument, ct).ConfigureAwait(false);
                refEditor.ReplaceNode(invocation, newInvocation);
                updatedSolution = refEditor.GetChangedDocument().Project.Solution;
            }
        }

        return updatedSolution.GetDocument(document.Id) ?? updatedDocument;
    }
}
