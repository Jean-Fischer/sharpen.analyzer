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
using Sharpen.Analyzer.Extensions;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.FixProvider.CSharp12;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseInlineArrayCodeFixProvider))]
[Shared]
public sealed class UseInlineArrayCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(CSharp12Rules.UseInlineArrayRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics[0];
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var @struct = node.FirstAncestorOrSelf<StructDeclarationSyntax>();
        if (@struct is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use InlineArray",
                ct => UseInlineArrayAsync(context.Document, @struct, diagnostic, ct),
                nameof(UseInlineArrayCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> UseInlineArrayAsync(Document document, StructDeclarationSyntax @struct,
        Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        if (!TryGetInlineArrayLength(diagnostic, out var length))
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
        var updatedStruct = BuildUpdatedStruct(currentStruct, length);

        editor.ReplaceNode(currentStruct, updatedStruct);
        return editor.GetChangedDocument();
    }

    private static bool TryGetInlineArrayLength(Diagnostic diagnostic, out int length)
    {
        length = 1;
        var message = diagnostic.GetMessage();
        if (message is null)
            return false;

        var start = message.IndexOf("InlineArray(", StringComparison.Ordinal);
        if (start < 0)
            return true;

        start += "InlineArray(".Length;
        var end = message.IndexOf(')', start);
        if (end > start && int.TryParse(message.Substring(start, end - start), out var parsed))
            length = parsed;

        return true;
    }

    private static StructDeclarationSyntax BuildUpdatedStruct(StructDeclarationSyntax currentStruct, int length)
    {
        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(CSharp12SyntaxFactory.InlineArrayAttribute(length)));
        var originalFields = currentStruct.Members.OfType<FieldDeclarationSyntax>().ToArray();

        var updatedStruct = originalFields.Length == 0
            ? currentStruct
            : currentStruct.WithMembers(RebuildMembers(currentStruct, CreateElement0Field(originalFields[0])));

        updatedStruct = updatedStruct.WithAttributeLists(currentStruct.AttributeLists.Insert(0, attributeList));
        return updatedStruct
            .WithLeadingTrivia(currentStruct.GetLeadingTrivia())
            .WithTrailingTrivia(currentStruct.GetTrailingTrivia())
            .WithOpenBraceToken(currentStruct.OpenBraceToken)
            .WithCloseBraceToken(currentStruct.CloseBraceToken);
    }

    private static FieldDeclarationSyntax CreateElement0Field(FieldDeclarationSyntax firstField)
    {
        return SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    firstField.Declaration.Type,
                    SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("_element0"))))
            .WithModifiers(firstField.Modifiers)
            .WithLeadingTrivia(firstField.GetLeadingTrivia())
            .WithTrailingTrivia(firstField.GetTrailingTrivia());
    }

    private static SyntaxList<MemberDeclarationSyntax> RebuildMembers(
        StructDeclarationSyntax currentStruct,
        FieldDeclarationSyntax element0Field)
    {
        var newMembers = new SyntaxList<MemberDeclarationSyntax>();
        var element0Inserted = false;

        foreach (var member in currentStruct.Members)
        {
            if (member is not FieldDeclarationSyntax)
            {
                newMembers = newMembers.Add(member);
                continue;
            }

            if (element0Inserted)
                continue;

            newMembers = newMembers.Add(element0Field);
            element0Inserted = true;
        }

        return newMembers;
    }
}
