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
        ImmutableArray.Create(Rules.GeneralRules.ReplaceSwitchStatementWithSwitchExpressionRule);

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
        if (!switchStatement.Sections.Any()) return;

        // Legacy behavior: do not support multiple labels per section.
        if (switchStatement.Sections.Any(section => section.Labels.Count != 1)) return;

        var isSurelyExhaustive = IsSurelyExhaustive(switchStatement.Sections);
        var diagnostic = GetDiagnostic(context.SemanticModel, switchStatement.Sections, isSurelyExhaustive);
        if (diagnostic is null) return;

        context.ReportDiagnostic(Diagnostic.Create(diagnostic, switchStatement.SwitchKeyword.GetLocation()));
    }

    private static bool AllSwitchSectionsAreAssignmentsToTheSameIdentifier(
        SemanticModel semanticModel,
        SyntaxList<SwitchSectionSyntax> switchSections)
    {
        ISymbol? previousIdentifierSymbol = null;

        foreach (var switchSection in switchSections)
        {
            if (IsThrowOnlySection(switchSection))
                continue;

            if (!TryGetAssignedIdentifierSymbol(semanticModel, switchSection, out var currentIdentifierSymbol))
                return false;

            if (previousIdentifierSymbol != null
                && !SymbolEqualityComparer.Default.Equals(previousIdentifierSymbol, currentIdentifierSymbol))
            {
                return false;
            }

            previousIdentifierSymbol = currentIdentifierSymbol;
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

    private static bool IsSurelyExhaustive(SyntaxList<SwitchSectionSyntax> switchSections)
    {
        return switchSections.Any(section => section.Labels.Any(label => label.IsKind(SyntaxKind.DefaultSwitchLabel)));
    }

    private static DiagnosticDescriptor? GetDiagnostic(
        SemanticModel semanticModel,
        SyntaxList<SwitchSectionSyntax> switchSections,
        bool isSurelyExhaustive)
    {
        if (AllSwitchSectionsAreAssignmentsToTheSameIdentifier(semanticModel, switchSections))
        {
            return isSurelyExhaustive
                ? Rules.GeneralRules.ReplaceSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule
                : Rules.GeneralRules.ConsiderReplacingSwitchStatementContainingOnlyAssignmentsWithSwitchExpressionRule;
        }

        return AllSwitchSectionsAreReturnStatements(switchSections)
            ? isSurelyExhaustive
                ? Rules.GeneralRules.ReplaceSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule
                : Rules.GeneralRules.ConsiderReplacingSwitchStatementContainingOnlyReturnsWithSwitchExpressionRule
            : null;
    }

    private static bool IsThrowOnlySection(SwitchSectionSyntax switchSection)
    {
        return switchSection.Statements.Count == 1
               && switchSection.Statements[0].IsKind(SyntaxKind.ThrowStatement);
    }

    private static bool TryGetAssignedIdentifierSymbol(
        SemanticModel semanticModel,
        SwitchSectionSyntax switchSection,
        out ISymbol? identifierSymbol)
    {
        identifierSymbol = null;

        if (switchSection.Statements.Count != 2
            || !switchSection.Statements[1].IsKind(SyntaxKind.BreakStatement)
            || switchSection.Statements[0] is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
            || !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            return false;
        }

        identifierSymbol = semanticModel.GetSymbolInfo(assignment.Left).Symbol;
        return identifierSymbol is not null;
    }
}
