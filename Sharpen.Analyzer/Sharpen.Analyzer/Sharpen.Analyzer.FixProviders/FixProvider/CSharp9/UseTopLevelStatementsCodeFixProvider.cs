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
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.FixProvider.CSharp9;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseTopLevelStatementsCodeFixProvider))]
[Shared]
public sealed class UseTopLevelStatementsCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(Rules.Rules.UseTopLevelStatementsRule.Id);

    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (!await IsCSharp9OrAboveAsync(context.Document, context.CancellationToken).ConfigureAwait(false))
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var programClass = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (programClass == null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                "Use top-level statements",
                c => ConvertAsync(context.Document, programClass, c),
                "UseTopLevelStatements"),
            diagnostic);
    }

    private static async Task<bool> IsCSharp9OrAboveAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp9OrAbove(compilation);
    }

    private static async Task<Document> ConvertAsync(Document document, ClassDeclarationSyntax programClass,
        CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
            return document;

        // Find Main method.
        var mainMethod = programClass.Members.OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.ValueText == "Main");
        if (mainMethod?.Body == null)
            return document;

        // Lift statements.
        var statements = mainMethod.Body.Statements;

        // Remove Program class from members.
        var newMembers = compilationUnit.Members.Where(m => !ReferenceEquals(m, programClass)).ToList();

        // In older Roslyn versions, top-level statements are represented as GlobalStatementSyntax in Members.
        var globalStatements = statements
            .Select(s => (MemberDeclarationSyntax)SyntaxFactory.GlobalStatement(s))
            .ToArray();

        var newCompilationUnit = compilationUnit.WithMembers(SyntaxFactory.List(newMembers.Concat(globalStatements)));

        // Preserve leading trivia from Program class by attaching it to the first lifted statement (if any).
        if (statements.Count > 0)
        {
            var firstGlobal = globalStatements[0];
            var withTrivia =
                firstGlobal.WithLeadingTrivia(programClass.GetLeadingTrivia().AddRange(firstGlobal.GetLeadingTrivia()));
            newCompilationUnit = newCompilationUnit.ReplaceNode(firstGlobal, withTrivia);
        }

        // If there are no statements, keep an empty file (just remove Program).
        return document.WithSyntaxRoot(newCompilationUnit);
    }
}