using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sharpen.Analyzer.Rules;
using Sharpen.Engine.SharpenSuggestions.Common.AsyncAwaitAndAsyncStreams;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AwaitEquivalentAsynchronousMethodCodeFixProvider))]
[Shared]
public class AwaitEquivalentAsynchronousMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.AwaitEquivalentAsynchronousMethodRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        var diagnostic = context.Diagnostics.First();
        var invocation = root.FindNode(diagnostic.Location.SourceSpan) as InvocationExpressionSyntax;
        if (invocation == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use async equivalent",
                createChangedDocument: c => ApplyAsyncEquivalentAsync(context.Document, invocation, c),
                equivalenceKey: "UseAsyncEquivalent"
            ),
            diagnostic
        );
    }

    private async Task<Document> ApplyAsyncEquivalentAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken ct)
    {
        var semanticModel = await document.GetSemanticModelAsync(ct);
        var method = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (method == null) return document;

        // Find the async equivalent (reuse your logic)
        var asyncMethodName = method.Name + "Async";
        var asyncMethod = method.ContainingType.GetMembers(asyncMethodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => IsAsyncEquivalent(m, method, semanticModel));

        if (asyncMethod == null) return document;

        // Rewrite the invocation to use the async method
        var newInvocation = RewriteInvocation(invocation, asyncMethodName);

        // Add `await` if the enclosing method is async
        var root = await document.GetSyntaxRootAsync(ct);
        var enclosingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (enclosingMethod != null)
        {
            var enclosingMethodSymbol = semanticModel.GetDeclaredSymbol(enclosingMethod);
            if (enclosingMethodSymbol.IsAsync)
            {
                newInvocation = SyntaxFactory.AwaitExpression(newInvocation)
                    .WithLeadingTrivia(invocation.GetLeadingTrivia())
                    .WithTrailingTrivia(invocation.GetTrailingTrivia());
            }
        }

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    private bool IsAsyncEquivalent(IMethodSymbol asyncMethod, IMethodSymbol syncMethod, SemanticModel semanticModel)
    {
        // Reuse your logic from `IsAsynchronousEquivalent` in `EquivalentAsynchronousMethodFinder`
        // (simplified for brevity)
        if (asyncMethod.ReturnType == null || syncMethod.ReturnType == null)
            return false;

        // Check return type compatibility
        var isReturnTypeCompatible = CheckReturnTypeCompatibility(asyncMethod, syncMethod, semanticModel);
        if (!isReturnTypeCompatible)
            return false;

        // Check parameter compatibility
        var isParameterCompatible = CheckParameterCompatibility(asyncMethod, syncMethod);
        if (!isParameterCompatible)
            return false;

        return true;
    }

    private bool CheckReturnTypeCompatibility(IMethodSymbol asyncMethod, IMethodSymbol syncMethod, SemanticModel semanticModel)
    {
        if (syncMethod.ReturnsVoid)
        {
            return EquivalentAsynchronousMethodFinder.KnownAwaitableTypes
                .Any(t => t.IsVoidEquivalent && t.RepresentsType(asyncMethod.ReturnType));
        }
        else
        {
            var asyncReturnType = asyncMethod.ReturnType as INamedTypeSymbol;
            if (asyncReturnType == null || asyncReturnType.TypeArguments.Length == 0)
                return false;

            return EquivalentAsynchronousMethodFinder.KnownAwaitableTypes
                .Any(t => t.RepresentsType(asyncReturnType.ConstructedFrom) &&
                          CheckGenericTypeCompatibility(t, asyncReturnType, syncMethod));
        }
    }

    private bool CheckGenericTypeCompatibility(
        EquivalentAsynchronousMethodFinder.KnownAwaitableTypeInfo awaitableType,
        INamedTypeSymbol asyncReturnType,
        IMethodSymbol syncMethod)
    {
        if (awaitableType.WrapsReturnType())
        {
            // For Task<T>, ValueTask<T>: T should match sync return type
            return syncMethod.ReturnType.Equals(asyncReturnType.TypeArguments[0]);
        }
        else // WrapsReturnTypeTypeParameter
        {
            // For IAsyncEnumerable<T>: T should match sync return type's generic parameter
            var syncReturnType = syncMethod.ReturnType as INamedTypeSymbol;
            if (syncReturnType == null || syncReturnType.TypeArguments.Length == 0)
                return false;

            return syncReturnType.TypeArguments[0].Equals(asyncReturnType.TypeArguments[0]);
        }
    }

    private SyntaxNode RewriteInvocation(InvocationExpressionSyntax invocation, string asyncMethodName)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            return invocation.WithExpression(
                memberAccess.WithName(SyntaxFactory.IdentifierName(asyncMethodName))
            );
        }
        else
        {
            return invocation.WithExpression(
                SyntaxFactory.IdentifierName(asyncMethodName)
            );
        }
    }
}
