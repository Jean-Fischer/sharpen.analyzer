using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp12;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseDefaultLambdaParametersAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp12Rules.UseDefaultLambdaParametersRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeSimpleLambda, SyntaxKind.SimpleLambdaExpression);
        context.RegisterSyntaxNodeAction(AnalyzeParenthesizedLambda, SyntaxKind.ParenthesizedLambdaExpression);
    }

    private static void AnalyzeSimpleLambda(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not SimpleLambdaExpressionSyntax lambda)
            return;

        // C# 12 default lambda parameters require explicit parameter types.
        if (lambda.Parameter.Type is null)
            return;

        // We only suggest when the lambda is converted to a delegate type with optional parameters.
        if (context.SemanticModel.GetTypeInfo(lambda).ConvertedType is not INamedTypeSymbol delegateType)
            return;

        if (delegateType.DelegateInvokeMethod is not { } invoke)
            return;

        if (invoke.Parameters.Length != 1)
            return;

        var p = invoke.Parameters[0];
        if (!p.HasExplicitDefaultValue)
            return;

        if (!IsSupportedDefaultValue(p))
            return;

        if (lambda.Parameter.Default is not null)
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp12Rules.UseDefaultLambdaParametersRule,
            lambda.Parameter.GetLocation()));
    }

    private static void AnalyzeParenthesizedLambda(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ParenthesizedLambdaExpressionSyntax lambda)
            return;

        // Parameterless lambdas have no parameters to annotate with defaults.
        if (!lambda.ParameterList.Parameters.Any())
            return;

        // C# 12 default lambda parameters require explicit parameter types.
        if (lambda.ParameterList.Parameters.Any(parameter => parameter.Type is null))
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(lambda).ConvertedType is not INamedTypeSymbol delegateType)
            return;

        if (delegateType.DelegateInvokeMethod is not { } invoke)
            return;

        if (invoke.Parameters.Length != lambda.ParameterList.Parameters.Count)
            return;

        for (var i = 0; i < invoke.Parameters.Length; i++)
        {
            var p = invoke.Parameters[i];
            if (!p.HasExplicitDefaultValue)
                return;

            if (!IsSupportedDefaultValue(p))
                return;

            // If the lambda already has a default for this parameter, don't report.
            if (lambda.ParameterList.Parameters[i].Default is not null)
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(CSharp12Rules.UseDefaultLambdaParametersRule,
            lambda.ParameterList.GetLocation()));
    }

    private static bool IsSupportedDefaultValue(IParameterSymbol parameter)
    {
        // Keep conservative: allow only compile-time constants and null.
        // (default parameter values in C# are limited; this matches most practical cases.)
        if (parameter.ExplicitDefaultValue is null)
            return true;

        return parameter.ExplicitDefaultValue is bool
            or byte or sbyte
            or short or ushort
            or int or uint
            or long or ulong
            or float or double
            or decimal
            or char
            or string;
    }
}