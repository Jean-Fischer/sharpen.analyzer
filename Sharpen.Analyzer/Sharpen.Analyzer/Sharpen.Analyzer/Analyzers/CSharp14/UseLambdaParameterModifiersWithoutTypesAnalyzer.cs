using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseLambdaParameterModifiersWithoutTypesAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeLambda, SyntaxKind.ParenthesizedLambdaExpression);
    }

    private static void AnalyzeLambda(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp14OrAbove(context.Compilation)) return;

        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;

        // Only consider lambdas with explicit parameter list.
        var parameterList = lambda.ParameterList;
        if (parameterList is null) return;

        // We only care about parameters that have BOTH:
        // - a modifier (ref/in/out/scoped/etc.)
        // - an explicit type
        // and where the type is redundant (i.e. can be inferred from target typing).
        var hasCandidate = false;
        foreach (var parameter in parameterList.Parameters)
        {
            if (parameter.Type is null) continue;

            if (parameter.Modifiers.Count == 0) continue;

            hasCandidate = true;
            break;
        }

        if (!hasCandidate) return;

        // Conservative: only report when the lambda is target-typed to a known delegate type.
        // This avoids overload-resolution changes in ambiguous contexts.
        var typeInfo = context.SemanticModel.GetTypeInfo(lambda, context.CancellationToken);
        if (typeInfo.ConvertedType is not INamedTypeSymbol { TypeKind: TypeKind.Delegate }) return;

        // Ensure this is *explicitly* target-typed (e.g. `SomeDelegate f = (...) => ...;`).
        // In `var f = (...) => ...;`, the lambda can still have a ConvertedType due to inference.
        if (lambda.Parent is not EqualsValueClauseSyntax equalsValueClause) return;

        if (equalsValueClause.Parent is not VariableDeclaratorSyntax variableDeclarator) return;

        if (variableDeclarator.Parent is not VariableDeclarationSyntax variableDeclaration) return;

        if (variableDeclaration.Type is IdentifierNameSyntax { Identifier.Text: "var" }) return;

        // If any parameter type is already implicit (null), we don't suggest.
        // If any parameter has a modifier but no type, it's already in the desired form.
        foreach (var parameter in parameterList.Parameters)
            if (parameter.Modifiers.Count > 0 && parameter.Type is null)
                return;

        // Report on the parameter list to keep the diagnostic stable.
        context.ReportDiagnostic(
            Diagnostic.Create(CSharp14Rules.UseLambdaParameterModifiersWithoutTypesRule, parameterList.GetLocation()));
    }
}