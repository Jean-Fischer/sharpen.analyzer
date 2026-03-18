using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseTopLevelStatementsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseTopLevelStatementsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        // SyntaxTreeAnalysisContext doesn't expose Compilation; use parse options for language version gating.
        if (context.Tree.Options is not CSharpParseOptions parseOptions ||
            parseOptions.LanguageVersion < LanguageVersion.CSharp9)
        {
            return;
        }

        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
            return;

        // Conservative: only when there is no namespace declaration.
        if (root.Members.OfType<BaseNamespaceDeclarationSyntax>().Any())
            return;

        // Conservative: avoid files with preprocessor directives.
        if (root.ContainsDirectives)
            return;

        // Must contain exactly one type: class Program.
        var typeDecls = root.Members.OfType<TypeDeclarationSyntax>().ToList();
        if (typeDecls.Count != 1)
            return;

        if (typeDecls[0] is not ClassDeclarationSyntax programClass)
            return;

        if (programClass.Identifier.ValueText != "Program")
            return;

        // No other members besides Main.
        var members = programClass.Members;
        if (members.Count != 1)
            return;

        if (members[0] is not MethodDeclarationSyntax mainMethod)
            return;

        if (mainMethod.Identifier.ValueText != "Main")
            return;

        // Must be static.
        if (!mainMethod.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
            return;

        // Must have a body (block). (We can extend to expression-bodied later.)
        if (mainMethod.Body == null)
            return;

        // Avoid local function/type declarations in Main body (conservative).
        if (mainMethod.Body.DescendantNodes().OfType<LocalFunctionStatementSyntax>().Any())
            return;

        if (mainMethod.Body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Any(d =>
                d.Declaration.Variables.Any(v => v.Initializer?.Value is AnonymousObjectCreationExpressionSyntax)))
        {
            // Not strictly required, but keep conservative around anonymous types.
            return;
        }

        // Avoid typeof(Program) usage anywhere in the file.
        if (root.DescendantNodes().OfType<TypeOfExpressionSyntax>()
            .Any(t => t.Type is IdentifierNameSyntax id && id.Identifier.ValueText == "Program"))
        {
            return;
        }

        // Avoid references to Program identifier (very conservative): if any IdentifierName "Program" exists.
        // This will also catch typeof(Program) but we already checked; keep it simple.
        if (root.DescendantNodes().OfType<IdentifierNameSyntax>().Any(i => i.Identifier.ValueText == "Program"))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTopLevelStatementsRule,
            programClass.Identifier.GetLocation()));
    }
}