using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFieldKeywordInPropertiesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseFieldKeywordInPropertiesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        // Only consider properties with explicit accessors.
        if (property.AccessorList is null) return;

        var getAccessor =
            property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        var setAccessor =
            property.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

        if (getAccessor is null || setAccessor is null) return;

        if (!TryGetBackingFieldFromGetter(context, getAccessor, out var backingFieldSymbol) ||
            backingFieldSymbol is null) return;

        if (!IsSimpleSetterAssigningField(context, setAccessor, backingFieldSymbol)) return;

        // Analyzer is intentionally conservative; safety checker will enforce cross-member constraints.
        context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseFieldKeywordInPropertiesRule,
            property.Identifier.GetLocation()));
    }

    private static bool TryGetBackingFieldFromGetter(
        SyntaxNodeAnalysisContext context,
        AccessorDeclarationSyntax getAccessor,
        out IFieldSymbol? backingFieldSymbol)
    {
        backingFieldSymbol = null;

        ExpressionSyntax? returnedExpression = null;

        if (getAccessor.ExpressionBody is not null)
        {
            returnedExpression = getAccessor.ExpressionBody.Expression;
        }
        else if (getAccessor.Body is not null)
        {
            var statements = getAccessor.Body.Statements;
            if (statements.Count != 1) return false;

            if (statements[0] is not ReturnStatementSyntax returnStatement) return false;

            returnedExpression = returnStatement.Expression;
        }

        if (returnedExpression is null) return false;

        var symbol = context.SemanticModel.GetSymbolInfo(returnedExpression, context.CancellationToken).Symbol;
        if (symbol is not IFieldSymbol fieldSymbol) return false;

        backingFieldSymbol = fieldSymbol;
        return true;
    }

    private static bool IsSimpleSetterAssigningField(
        SyntaxNodeAnalysisContext context,
        AccessorDeclarationSyntax setAccessor,
        IFieldSymbol backingFieldSymbol)
    {
        ExpressionSyntax? assignedExpression = null;

        if (setAccessor.ExpressionBody is not null)
        {
            assignedExpression = setAccessor.ExpressionBody.Expression;
        }
        else if (setAccessor.Body is not null)
        {
            var statements = setAccessor.Body.Statements;
            if (statements.Count != 1) return false;

            if (statements[0] is not ExpressionStatementSyntax expressionStatement) return false;

            assignedExpression = expressionStatement.Expression;
        }

        if (assignedExpression is not AssignmentExpressionSyntax assignment ||
            !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            return false;

        var leftSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol;
        if (!SymbolEqualityComparer.Default.Equals(leftSymbol, backingFieldSymbol)) return false;

        // Require assignment from 'value'.
        var rightSymbol = context.SemanticModel.GetSymbolInfo(assignment.Right, context.CancellationToken).Symbol;
        return rightSymbol is IParameterSymbol { Name: "value" };
    }
}