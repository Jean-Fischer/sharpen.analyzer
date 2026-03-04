using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFileScopedNamespaceAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseFileScopedNamespaceRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        // SyntaxTreeAnalysisContext doesn't expose Compilation; use parse options for language version gating.
        if (context.Tree.Options is not CSharpParseOptions parseOptions || parseOptions.LanguageVersion < LanguageVersion.CSharp10)
            return;

        var root = context.Tree.GetRoot(context.CancellationToken);

        // Only suggest when there is exactly one top-level namespace declaration.
        var namespaces = root.DescendantNodes(n => n is CompilationUnitSyntax or NamespaceDeclarationSyntax)
            .OfType<NamespaceDeclarationSyntax>()
            .Where(n => n.Parent is CompilationUnitSyntax)
            .ToArray();

        if (namespaces.Length != 1)
            return;

        var ns = namespaces[0];

        // Only block-scoped namespaces are eligible.
        if (ns.NamespaceKeyword.IsKind(SyntaxKind.None))
            return;

        // If already file-scoped (C# 10 has FileScopedNamespaceDeclarationSyntax), this analyzer won't see it.
        // But keep a defensive check in case of syntax changes.
        if (ns is FileScopedNamespaceDeclarationSyntax)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp10Rules.UseFileScopedNamespaceRule, ns.Name.GetLocation()));
    }
}
