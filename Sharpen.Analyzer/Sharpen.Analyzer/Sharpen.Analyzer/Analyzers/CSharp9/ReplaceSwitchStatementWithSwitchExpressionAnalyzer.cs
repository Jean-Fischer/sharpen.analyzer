using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceSwitchStatementWithSwitchExpressionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.ReplaceSwitchStatementWithSwitchExpressionRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
    }

    private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;

        // We have to have at least one switch section (case or default).
        if (switchStatement.Sections.Count <= 0) return;

        // Legacy behavior: do not support multiple labels per section.
        if (switchStatement.Sections.Any(section => section.Labels.Count != 1)) return;

        var isSurelyExhaustive = switchStatement.Sections.Any(section =>
            section.Labels.Any(label => label.IsKind(SyntaxKind.DefaultSwitchLabel)));

        if (AllSwitchSectionsAreAssignmentsToTheSameIdentifier(context.SemanticModel, switchStatement.Sections))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                isSurelyExhaustive
                    ? Rules.Rules.ReplaceSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule
                    : Rules.Rules.ConsiderReplacingSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule,
                switchStatement.SwitchKeyword.GetLocation()));
            return;
        }

        if (AllSwitchSectionsAreReturnStatements(switchStatement.Sections))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                isSurelyExhaustive
                    ? Rules.Rules.ReplaceSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule
                    : Rules.Rules.ConsiderReplacingSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule,
                switchStatement.SwitchKeyword.GetLocation()));
        }
    }

    private static bool AllSwitchSectionsAreAssignmentsToTheSameIdentifier(
        SemanticModel semanticModel,
        SyntaxList<SwitchSectionSyntax> switchSections)
    {
        ISymbol? previousIdentifierSymbol = null;

        foreach (var switchSection in switchSections)
        {
            switch (switchSection.Statements.Count)
            {
                // We have only one statement which then must be exception throwing.
                case 1:
                    if (!switchSection.Statements[0].IsKind(SyntaxKind.ThrowStatement)) return false;
                    break;

                // We have two statements, which then must be an assignment immediately followed by break.
                case 2:
                    if (!switchSection.Statements[1].IsKind(SyntaxKind.BreakStatement)) return false;
                    if (switchSection.Statements[0] is not ExpressionStatementSyntax expression) return false;
                    if (expression.Expression is not AssignmentExpressionSyntax assignment) return false;

                    // Legacy behavior: do not support compound assignments (+=, *=, -=, /=, ...).
                    if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)) return false;

                    if (assignment.Left == null) return false;

                    var currentIdentifierSymbol = semanticModel.GetSymbolInfo(assignment.Left).Symbol;
                    if (currentIdentifierSymbol == null) return false;

                    if (previousIdentifierSymbol != null && !previousIdentifierSymbol.Equals(currentIdentifierSymbol)) return false;

                    previousIdentifierSymbol = currentIdentifierSymbol;
                    break;

                default:
                    return false;
            }
        }

        return true;
    }

    private static bool AllSwitchSectionsAreReturnStatements(SyntaxList<SwitchSectionSyntax> switchSections)
    {
        foreach (var switchSection in switchSections)
        {
            // Valid cases are either throwing an exception or having return.
            // In both cases we expect exactly one statement.
            if (switchSection.Statements.Count != 1) return false;

            switch (switchSection.Statements[0].Kind())
            {
                case SyntaxKind.ReturnStatement:
                    var returnStatement = (ReturnStatementSyntax)switchSection.Statements[0];
                    if (returnStatement.Expression == null) return false;
                    break;

                case SyntaxKind.ThrowStatement:
                    break;

                default:
                    return false;
            }
        }

        return true;
    }
}
