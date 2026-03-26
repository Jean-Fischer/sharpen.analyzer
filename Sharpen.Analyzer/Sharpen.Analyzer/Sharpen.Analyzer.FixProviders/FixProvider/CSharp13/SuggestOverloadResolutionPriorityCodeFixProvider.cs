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
using Microsoft.CodeAnalysis.Formatting;
using Sharpen.Analyzer.FixProvider.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProviders.FixProvider.CSharp13;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuggestOverloadResolutionPriorityCodeFixProvider))]
[Shared]
public sealed class SuggestOverloadResolutionPriorityCodeFixProvider : CSharp13OrAboveSharpenCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp13Rules.SuggestOverloadResolutionPriorityRule.Id);

    protected override async Task RegisterCodeFixesAsync(CodeFixContext context, SyntaxNode root, Diagnostic diagnostic)
    {
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        var method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Add OverloadResolutionPriorityAttribute (requires review)",
                ct => AddAttributeAsync(context.Document, method, ct),
                "AddOverloadResolutionPriorityAttribute"),
            diagnostic);
    }

    private static async Task<Document> AddAttributeAsync(Document document, MethodDeclarationSyntax method,
        CancellationToken ct)
    {
        // Avoid adding duplicates if the user runs the fix multiple times.
        if (method.AttributeLists.SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString().Contains("OverloadResolutionPriority")))
        {
            return document;
        }

        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

        // Default suggested priority. Users should review and adjust.
        const int priority = 1;

        var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("System.Runtime.CompilerServices.OverloadResolutionPriority"))
            .WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                SyntaxFactory.Literal(priority))))));

        var list = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));

        var newMethod = method.AddAttributeLists(list)
            .WithAdditionalAnnotations(Formatter.Annotation);

        editor.ReplaceNode(method, newMethod);

        return editor.GetChangedDocument();
    }
}