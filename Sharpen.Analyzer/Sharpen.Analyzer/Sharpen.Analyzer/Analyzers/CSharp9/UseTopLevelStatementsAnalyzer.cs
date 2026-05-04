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
        ImmutableArray.Create(Rules.GeneralRules.UseTopLevelStatementsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (!SupportsTopLevelStatements(context))
            return;

        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax root)
            return;

        if (!TryGetProgramMain(root, out var programClass, out var mainMethod))
            return;

        if (MainBodyContainsUnsupportedConstructs(mainMethod) || ReferencesProgramIdentifier(root))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.GeneralRules.UseTopLevelStatementsRule,
            programClass.Identifier.GetLocation()));
    }

    private static bool SupportsTopLevelStatements(SyntaxTreeAnalysisContext context)
    {
        return context.Tree.Options is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp9 };
    }

    private static bool TryGetProgramMain(
        CompilationUnitSyntax root,
        out ClassDeclarationSyntax programClass,
        out MethodDeclarationSyntax mainMethod)
    {
        programClass = null!;
        mainMethod = null!;

        if (root.Members.OfType<BaseNamespaceDeclarationSyntax>().Any() || root.ContainsDirectives)
            return false;

        var typeDecls = root.Members.OfType<TypeDeclarationSyntax>().ToList();
        if (typeDecls.Count != 1 || typeDecls[0] is not ClassDeclarationSyntax candidate)
            return false;

        if (candidate.Identifier.ValueText != "Program" || candidate.Members.Count != 1)
            return false;

        if (candidate.Members[0] is not MethodDeclarationSyntax method
            || method.Identifier.ValueText != "Main"
            || !method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            || method.Body is null)
        {
            return false;
        }

        programClass = candidate;
        mainMethod = method;
        return true;
    }

    private static bool MainBodyContainsUnsupportedConstructs(MethodDeclarationSyntax mainMethod)
    {
        return mainMethod.Body!.DescendantNodes().OfType<LocalFunctionStatementSyntax>().Any()
               || mainMethod.Body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Any(DeclaresAnonymousObject);
    }

    private static bool DeclaresAnonymousObject(LocalDeclarationStatementSyntax declaration)
    {
        return declaration.Declaration.Variables.Any(v => v.Initializer?.Value is AnonymousObjectCreationExpressionSyntax);
    }

    private static bool ReferencesProgramIdentifier(CompilationUnitSyntax root)
    {
        return ContainsProgramTypeOf(root) || ContainsProgramIdentifier(root);
    }

    private static bool ContainsProgramTypeOf(CompilationUnitSyntax root)
    {
        return root.DescendantNodes().OfType<TypeOfExpressionSyntax>()
            .Any(t => t.Type is IdentifierNameSyntax id && id.Identifier.ValueText == "Program");
    }

    private static bool ContainsProgramIdentifier(CompilationUnitSyntax root)
    {
        return root.DescendantNodes().OfType<IdentifierNameSyntax>().Any(i => i.Identifier.ValueText == "Program");
    }
}
