using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp14;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseUnboundGenericTypeInNameofAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp14Rules.UseUnboundGenericTypeInNameofRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp14OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzeNameof, SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeNameof(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
            return;

        if (invocation.ArgumentList.Arguments.Count != 1)
            return;

        var argExpression = invocation.ArgumentList.Arguments[0].Expression;

        // We only care about nameof(SomeGeneric<...>) where the argument is a type.
        if (argExpression is not TypeOfExpressionSyntax && argExpression is not IdentifierNameSyntax && argExpression is not GenericNameSyntax && argExpression is not QualifiedNameSyntax && argExpression is not AliasQualifiedNameSyntax)
        {
            // Still allow other TypeSyntax shapes via semantic model below.
        }

        var semanticModel = context.SemanticModel;
        var typeInfo = semanticModel.GetTypeInfo(argExpression, context.CancellationToken);

        if (typeInfo.Type is not INamedTypeSymbol namedType)
            return;

        if (!namedType.IsGenericType)
            return;

        // Only when the syntax is a constructed generic type (i.e., has type arguments).
        // nameof(Dictionary<,>) is already unbound and should not be flagged.
        if (argExpression is TypeSyntax typeSyntax)
        {
            if (typeSyntax is GenericNameSyntax genericName && genericName.TypeArgumentList.Arguments.Count > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseUnboundGenericTypeInNameofRule, typeSyntax.GetLocation()));
                return;
            }

            if (typeSyntax is QualifiedNameSyntax { Right: GenericNameSyntax rightGeneric } && rightGeneric.TypeArgumentList.Arguments.Count > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseUnboundGenericTypeInNameofRule, typeSyntax.GetLocation()));
                return;
            }

            if (typeSyntax is AliasQualifiedNameSyntax { Name: GenericNameSyntax aliasGeneric } && aliasGeneric.TypeArgumentList.Arguments.Count > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(CSharp14Rules.UseUnboundGenericTypeInNameofRule, typeSyntax.GetLocation()));
                return;
            }
        }
    }
}
