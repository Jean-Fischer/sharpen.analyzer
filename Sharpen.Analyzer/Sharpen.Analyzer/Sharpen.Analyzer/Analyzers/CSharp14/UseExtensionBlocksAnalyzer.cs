using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExtensionBlocksAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseExtensionBlocksRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp14OrAbove(context.Compilation))
            return;

        if (context.Node is not ClassDeclarationSyntax classDeclaration)
            return;

        if (!classDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
            return;

        // Only consider same-file methods in the class declaration.
        // We keep this informational and conservative.
        var extensionMethods = classDeclaration.Members
            .Where(m => m is MethodDeclarationSyntax)
            .Cast<MethodDeclarationSyntax>()
            .Where(m => m.ParameterList.Parameters.Count > 0)
            .Where(m => m.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
            .ToList();

        if (extensionMethods.Count < 2)
            return;

        // Group by receiver type syntax string (good enough for an informational hint).
        // We avoid semantic model heavy lifting here.
        var dominantGroup = extensionMethods
            .GroupBy(m => m.ParameterList.Parameters[0].Type?.ToString() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (dominantGroup is null)
            return;

        // Trigger when at least 2 extension methods share the same receiver type.
        if (dominantGroup.Count() < 2)
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseExtensionBlocksRule,
            classDeclaration.Identifier.GetLocation()));
    }
}