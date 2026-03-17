using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp10;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExtendedPropertyPatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp10Rules.UseExtendedPropertyPatternRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAndPattern, SyntaxKind.LogicalAndExpression);
        context.RegisterSyntaxNodeAction(AnalyzeNullConditionalEquality, SyntaxKind.EqualsExpression);
    }

    private static void AnalyzeAndPattern(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var andExpr = (BinaryExpressionSyntax)context.Node;

        // Pattern: x is T t && t.Prop OP value
        if (andExpr.Left is not IsPatternExpressionSyntax isPattern)
            return;

        if (isPattern.Pattern is not DeclarationPatternSyntax declPattern)
            return;

        if (declPattern.Designation is not SingleVariableDesignationSyntax designation)
            return;

        var identifier = designation.Identifier.ValueText;
        if (string.IsNullOrWhiteSpace(identifier))
            return;

        if (andExpr.Right is not BinaryExpressionSyntax rightBinary)
            return;

        // Only support relational/equality operators.
        if (rightBinary.Kind() is not (SyntaxKind.EqualsExpression
            or SyntaxKind.NotEqualsExpression
            or SyntaxKind.GreaterThanExpression
            or SyntaxKind.GreaterThanOrEqualExpression
            or SyntaxKind.LessThanExpression
            or SyntaxKind.LessThanOrEqualExpression))
            return;

        if (rightBinary.Left is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Expression is not IdentifierNameSyntax id || id.Identifier.ValueText != identifier)
            return;

        // Only simple property access (no nested chain here).
        if (memberAccess.Name is not IdentifierNameSyntax)
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp10Rules.UseExtendedPropertyPatternRule,
            andExpr.GetLocation()));
    }

    private static void AnalyzeNullConditionalEquality(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var equals = (BinaryExpressionSyntax)context.Node;

        // Pattern: x?.A?.B == c
        if (equals.Left is not ConditionalAccessExpressionSyntax)
            return;

        // Ensure the conditional chain is purely member access and ends in a member binding.
        if (!TryGetNullConditionalChain(equals.Left, out var rootExpr, out var members))
            return;

        // Avoid side effects: root must be identifier.
        if (rootExpr is not IdentifierNameSyntax)
            return;

        if (members.Length < 2)
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp10Rules.UseExtendedPropertyPatternRule, equals.GetLocation()));
    }

    private static bool TryGetNullConditionalChain(ExpressionSyntax expr, out ExpressionSyntax root,
        out ImmutableArray<string> members)
    {
        // Parse x?.A?.B into root=x and members=["A","B"].
        var builder = ImmutableArray.CreateBuilder<string>();

        var current = expr;
        while (current is ConditionalAccessExpressionSyntax ca)
        {
            // WhenNotNull should be MemberBindingExpressionSyntax for x?.A and for nested.
            if (ca.WhenNotNull is not MemberBindingExpressionSyntax mb)
            {
                root = expr;
                members = default;
                return false;
            }

            builder.Insert(0, mb.Name.Identifier.ValueText);
            current = ca.Expression;
        }

        root = current;
        members = builder.ToImmutable();
        return members.Length > 0;
    }
}