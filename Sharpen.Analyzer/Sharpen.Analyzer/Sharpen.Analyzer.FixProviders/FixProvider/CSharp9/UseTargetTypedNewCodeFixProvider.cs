using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.FixProvider.Common;
using CSharpLanguageVersion = Sharpen.Analyzer.Common.CSharpLanguageVersion;

namespace Sharpen.Analyzer.FixProvider.CSharp9;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTargetTypedNewCodeFixProvider))]
[Shared]
public sealed class UseTargetTypedNewCodeFixProvider : SharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.GeneralRules.UseTargetTypedNewRule.Id);

    protected override async Task<bool> ShouldRegisterFixesAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    protected override Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var objectCreation = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<ObjectCreationExpressionSyntax>();

        if (objectCreation is null)
            return Task.CompletedTask;

        RegisterCodeFix(
            context,
            diagnostic,
            "Use target-typed new",
            "UseTargetTypedNew",
            c => ApplyFixAsync(context.Document, objectCreation, c));

        return Task.CompletedTask;
    }

    private static async Task<Document> ApplyFixAsync(Document document, ObjectCreationExpressionSyntax objectCreation,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // new T(args) => new(args)
        // new T() { ... } => new() { ... }
        var argumentList = objectCreation.ArgumentList ?? SyntaxFactory.ArgumentList();
        var newExpression = SyntaxFactory.ImplicitObjectCreationExpression(
                SyntaxFactory.Token(SyntaxKind.NewKeyword),
                argumentList,
                objectCreation.Initializer)
            .WithTriviaFrom(objectCreation);

        var newRoot = root.ReplaceNode(objectCreation, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}