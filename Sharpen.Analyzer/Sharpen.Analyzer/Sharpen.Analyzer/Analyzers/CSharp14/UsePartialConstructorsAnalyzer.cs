using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePartialConstructorsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UsePartialConstructorsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ConstructorDeclarationSyntax ctor)
            return;

        if (ctor.Body is null)
            return;

        // Heuristic: constructor delegates initialization to a partial method.
        // We keep this informational and conservative.
        foreach (var statement in ctor.Body.Statements)
        {
            if (statement is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
                continue;

            if (invocation.Expression is not IdentifierNameSyntax identifier)
                continue;

            var name = identifier.Identifier.ValueText;
            if (!LooksLikeGeneratedInitializationMethodName(name))
                continue;

            if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol symbol)
                continue;

            if (!symbol.IsPartialDefinition)
                continue;

            context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UsePartialConstructorsRule,
                ctor.Identifier.GetLocation()));
            return;
        }
    }

    private static bool LooksLikeGeneratedInitializationMethodName(string name)
    {
        // Common patterns in source generation / partial-method initialization.
        return name is "InitializeGenerated" or "OnConstructed" or "Initialize" or "InitializeComponent";
    }
}