using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFromEndIndexInObjectInitializersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.UseFromEndIndexInObjectInitializersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeInitializer, SyntaxKind.ObjectInitializerExpression);
    }

    private static void AnalyzeInitializer(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InitializerExpressionSyntax initializer)
            return;

        foreach (var expression in initializer.Expressions)
        {
            // Object initializer indexer assignment is represented as an ImplicitElementAccess
            // on the LHS of a simple assignment:
            //   new C { [a.Length - 1] = 42 }
            //     -> AssignmentExpressionSyntax
            //        Left: ImplicitElementAccessSyntax
            //              ArgumentList: BracketedArgumentListSyntax
            if (expression is not AssignmentExpressionSyntax assignment)
                continue;

            if (assignment.Left is not ImplicitElementAccessSyntax implicitElementAccess)
                continue;

            // Only handle single-argument indexers: [expr] = ...
            if (implicitElementAccess.ArgumentList.Arguments.Count != 1)
                continue;

            var indexExpression = implicitElementAccess.ArgumentList.Arguments[0].Expression;
            if (indexExpression is null)
                continue;

            if (!IsLengthMinusOne(indexExpression))
                continue;

            // Ensure the `.Length` is from an array (per spec/tests).
            if (indexExpression is not BinaryExpressionSyntax { Left: MemberAccessExpressionSyntax memberAccess })
                continue;

            var lengthTargetType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken)
                .Type;
            if (lengthTargetType is null || lengthTargetType.TypeKind != TypeKind.Array)
                continue;

            // Do not gate analyzer diagnostics on fix-provider safety.
            // Fix availability is handled by the code fix provider + safety checker.

            context.ReportDiagnostic(Diagnostic.Create(
                CSharp13Rules.UseFromEndIndexInObjectInitializersRule,
                indexExpression.GetLocation()));
        }
    }

    private static bool IsLengthMinusOne(ExpressionSyntax expression)
    {
        // Match: <expr>.Length - 1
        if (expression is not BinaryExpressionSyntax { RawKind: (int)SyntaxKind.SubtractExpression } subtract)
            return false;

        if (subtract.Right is not LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NumericLiteralExpression } literal)
            return false;

        if (literal.Token.ValueText != "1")
            return false;

        if (subtract.Left is not MemberAccessExpressionSyntax memberAccess)
            return false;

        return memberAccess.Name.Identifier.ValueText == "Length";
    }
}