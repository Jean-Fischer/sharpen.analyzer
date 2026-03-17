using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp8;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceUsingStatementWithUsingDeclarationAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.ReplaceUsingStatementWithUsingDeclarationRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
    }

    private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
    {
        var usingStatement = (UsingStatementSyntax)context.Node;

        // Only offer on the topmost using statement in a nested chain.
        // (Nested using statements are represented as using statements inside the Statement of another using.)
        var outermostUsing = usingStatement;
        while (outermostUsing.Parent is UsingStatementSyntax parentUsing)
            outermostUsing = parentUsing;

        // Only offer on a using statement that is directly inside a block.
        if (outermostUsing.Parent is not BlockSyntax parentBlock) return;

        var innermostUsing = outermostUsing;

        // Check that all the immediately nested usings are convertible as well.
        // We don't want take a sequence of nested-using and only convert some of them.
        for (var current = outermostUsing; current != null; current = current.Statement as UsingStatementSyntax)
        {
            innermostUsing = current;
            if (current.Declaration == null) return;

            // Be conservative: converting to using declarations changes the scope boundary.
            // If the using body contains control-flow that could cross that boundary, do not offer.
            if (ContainsDisallowedControlFlow(current.Statement)) return;
        }

        var parentStatements = parentBlock.Statements;
        var index = parentStatements.IndexOf(outermostUsing);
        if (index < 0) return;

        // TODO: At the moment, if we have jumps (OMG!) we will simply not deal with it at all.
        if (!UsingStatementDoesNotInvolveJumps(parentStatements, index, innermostUsing)) return;

        // Everything is fine, the using statement *could* be replaced with the using declaration.
        // The potential leakage of the using value determines if it is 100% safe to do it or not.
        if (!UsingValueDoesNotLeakToFollowingStatements(parentStatements, index)) return;

        var diagnostic = Diagnostic.Create(
            Rules.Rules.ReplaceUsingStatementWithUsingDeclarationRule,
            outermostUsing.UsingKeyword.GetLocation());

        context.ReportDiagnostic(diagnostic);

        static bool ContainsDisallowedControlFlow(StatementSyntax statement)
        {
            // Disallow any jump/yield inside the using body. This is intentionally conservative.
            // (e.g. yield return/yield break are illegal across using declarations)
            return statement.DescendantNodesAndSelf().Any(node =>
                node.IsKind(SyntaxKind.GotoStatement) ||
                node.IsKind(SyntaxKind.LabeledStatement) ||
                node.IsKind(SyntaxKind.BreakStatement) ||
                node.IsKind(SyntaxKind.ContinueStatement) ||
                node.IsKind(SyntaxKind.ReturnStatement) ||
                node.IsKind(SyntaxKind.YieldBreakStatement) ||
                node.IsKind(SyntaxKind.YieldReturnStatement));
        }
    }

    private static bool UsingValueDoesNotLeakToFollowingStatements(SyntaxList<StatementSyntax> parentStatements,
        int index)
    {
        // Has to be one of the following forms:
        // 1. Using statement is the last statement in the parent.
        // 2. Using statement is not the last statement in parent, but is followed by
        //    something that is unaffected by simplifying the using statement.  I.e.
        //    `return`/`break`/`continue`.  *Note*.  `return expr` would *not* be ok.
        //    In that case, `expr` would now be evaluated *before* the using disposed
        //    the resource, instead of afterwards.

        // Very last statement in the block. Can be converted.
        if (index == parentStatements.Count - 1) return true;

        // Not the last statement, get the next statement and examine that.
        var nextStatement = parentStatements[index + 1];

        // Using statement followed by break/continue.
        if (nextStatement is BreakStatementSyntax || nextStatement is ContinueStatementSyntax) return true;

        // Using statement followed by `return` (no expression).
        if (nextStatement is ReturnStatementSyntax returnStatement && returnStatement.Expression == null) return true;

        return false;
    }

    private static bool UsingStatementDoesNotInvolveJumps(
        SyntaxList<StatementSyntax> parentStatements,
        int index,
        UsingStatementSyntax innermostUsing)
    {
        // Jumps are not allowed to cross a using declaration in the forward direction,
        // and can't go back unless there is a curly brace between the using and the label.
        // We conservatively implement this by disallowing the change if there are gotos/labels
        // in the containing block, or inside the using body.

        // Note: we only have to check up to the `using`, since the checks in
        // UsingValueDoesNotLeakToFollowingStatements ensure that there would be no
        // labels/gotos *after* the using statement.
        for (var i = 0; i < index; i++)
        {
            var priorStatement = parentStatements[i];
            if (IsGotoOrLabeledStatement(priorStatement)) return false;
        }

        var innerStatements = innermostUsing.Statement is BlockSyntax block
            ? block.Statements
            : new SyntaxList<StatementSyntax>().Add(innermostUsing.Statement);

        foreach (var statement in innerStatements)
            if (IsGotoOrLabeledStatement(statement))
                return false;

        return true;

        static bool IsGotoOrLabeledStatement(StatementSyntax statement)
        {
            return statement.Kind() == SyntaxKind.GotoStatement || statement.Kind() == SyntaxKind.LabeledStatement;
        }
    }
}