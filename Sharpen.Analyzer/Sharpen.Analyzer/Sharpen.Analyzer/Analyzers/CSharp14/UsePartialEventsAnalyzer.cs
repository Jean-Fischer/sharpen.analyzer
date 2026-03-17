using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePartialEventsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UsePartialEventsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeEvent, SyntaxKind.EventDeclaration);
    }

    private static void AnalyzeEvent(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not EventDeclarationSyntax eventDeclaration)
            return;

        if (eventDeclaration.AccessorList is null)
            return;

        var addAccessor =
            eventDeclaration.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.AddAccessorDeclaration));
        var removeAccessor =
            eventDeclaration.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.RemoveAccessorDeclaration));

        if (addAccessor?.Body is null || removeAccessor?.Body is null)
            return;

        if (!AccessorBodyCallsPartialMethod(context, addAccessor.Body))
            return;

        if (!AccessorBodyCallsPartialMethod(context, removeAccessor.Body))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UsePartialEventsRule,
            eventDeclaration.Identifier.GetLocation()));
    }

    private static bool AccessorBodyCallsPartialMethod(SyntaxNodeAnalysisContext context, BlockSyntax body)
    {
        // Conservative: require exactly one statement, which is a direct invocation of an identifier.
        if (body.Statements.Count != 1)
            return false;

        if (body.Statements[0] is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
            return false;

        if (invocation.Expression is not IdentifierNameSyntax)
            return false;

        var symbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
        return symbol is not null && symbol.IsPartialDefinition;
    }
}