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
using Sharpen.Analyzer.Extensions;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseInlineArrayCodeFixProvider))]
[Shared]
public sealed class UseInlineArrayCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UseInlineArrayRule.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var @struct = node.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (@struct is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use InlineArray",
                createChangedDocument: ct => UseInlineArrayAsync(context.Document, @struct, diagnostic, ct),
                equivalenceKey: nameof(UseInlineArrayCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> UseInlineArrayAsync(Document document, StructDeclarationSyntax @struct, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        if (diagnostic.GetMessage() is null)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

        // Work on the current struct node from the editor's syntax root (the passed-in node can be stale
        // during iterative code-fix application).
        var currentRoot = editor.OriginalRoot;
        if (currentRoot is null)
            return document;

        var currentStruct = currentRoot.FindNode(@struct.Span, getInnermostNodeForTie: true)
            .FirstAncestorOrSelf<StructDeclarationSyntax>()
            ?? @struct;

        // Diagnostic message argument 0 is N.
        var n = 1;
        if (diagnostic.Descriptor.MessageFormat is not null)
        {
            // We don't have access to Diagnostic.Arguments here (internal), so parse from the message.
            // Message format: "Use [InlineArray({0})] for this fixed-size buffer struct"
            var message = diagnostic.GetMessage();
            var start = message.IndexOf("InlineArray(", System.StringComparison.Ordinal);
            if (start >= 0)
            {
                start += "InlineArray(".Length;
                var end = message.IndexOf(')', start);
                if (end > start && int.TryParse(message.Substring(start, end - start), out var parsed))
                    n = parsed;
            }
        }

        // Add [System.Runtime.CompilerServices.InlineArray(N)]
        var attribute = CSharp12SyntaxFactory.InlineArrayAttribute(n);

        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute));

        // Normalize fields to a single `_element0` field.
        // NOTE: We rebuild the member list instead of trying to remove nodes from the existing list.
        // In iterative code-fix application, node identity can drift and `Remove` may no-op.
        var originalFields = currentStruct.Members.OfType<FieldDeclarationSyntax>().ToArray();
        if (originalFields.Length == 0)
        {
            var updatedStructWithoutFields = currentStruct.WithAttributeLists(currentStruct.AttributeLists.Insert(0, attributeList));
            editor.ReplaceNode(currentStruct, updatedStructWithoutFields);
            return editor.GetChangedDocument();
        }

        var firstField = originalFields[0];
        var elementType = firstField.Declaration.Type;

        var element0Field = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    elementType,
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("_element0"))))
            .WithModifiers(firstField.Modifiers)
            .WithLeadingTrivia(firstField.GetLeadingTrivia())
            .WithTrailingTrivia(firstField.GetTrailingTrivia());

        var newMembers = new SyntaxList<MemberDeclarationSyntax>();
        var element0Inserted = false;

        foreach (var member in currentStruct.Members)
        {
            if (member is FieldDeclarationSyntax)
            {
                if (!element0Inserted)
                {
                    newMembers = newMembers.Add(element0Field);
                    element0Inserted = true;
                }

                // Skip all original fields.
                continue;
            }

            newMembers = newMembers.Add(member);
        }

        var updatedStruct = currentStruct
            .WithMembers(newMembers)
            .WithAttributeLists(currentStruct.AttributeLists.Insert(0, attributeList));

        // Preserve the original struct's trivia so formatting stays stable.
        updatedStruct = updatedStruct
            .WithLeadingTrivia(currentStruct.GetLeadingTrivia())
            .WithTrailingTrivia(currentStruct.GetTrailingTrivia())
            .WithOpenBraceToken(currentStruct.OpenBraceToken)
            .WithCloseBraceToken(currentStruct.CloseBraceToken);

        editor.ReplaceNode(currentStruct, updatedStruct);
        return editor.GetChangedDocument();
    }
}
