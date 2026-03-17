using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuggestOverloadResolutionPriorityAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.SuggestOverloadResolutionPriorityRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.RecordDeclaration);
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;

        // Guidance-only heuristic:
        // Flag when a type contains multiple overloads with the same name and arity,
        // and at least one overload is a "catch-all" (params object[])
        // which can steal calls from more specific overloads.
        //
        // Note: we intentionally avoid span-based patterns here because the analyzer assembly
        // targets netstandard2.0 and does not have System.Index / newer BCL types available.

        var methods = typeDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.ValueText.Length > 0)
            // If the attribute is already present on any overload, don't keep reporting.
            // This keeps the code fix idempotent in a single iteration.
            .Where(m => !HasOverloadResolutionPriorityAttribute(m))
            .ToList();

        if (methods.Count < 2)
            return;

        var groups = methods.GroupBy(m =>
            (Name: m.Identifier.ValueText, Arity: m.TypeParameterList?.Parameters.Count ?? 0));

        foreach (var group in groups)
        {
            var overloads = group.ToList();
            if (overloads.Count < 2)
                continue;

            if (!HasCatchAllParamsObjectArrayOverload(overloads, context.SemanticModel, context.CancellationToken))
                continue;

            // Report on the method name token of the first overload to keep it stable.
            context.ReportDiagnostic(Diagnostic.Create(
                CSharp13Rules.SuggestOverloadResolutionPriorityRule,
                overloads[0].Identifier.GetLocation()));
        }
    }

    private static bool HasCatchAllParamsObjectArrayOverload(
        List<MethodDeclarationSyntax> overloads,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (var method in overloads)
        {
            if (method.ParameterList.Parameters.Count == 0)
                continue;

            var last = method.ParameterList.Parameters[method.ParameterList.Parameters.Count - 1];
            if (!last.Modifiers.Any(SyntaxKind.ParamsKeyword))
                continue;

            if (last.Type is null)
                continue;

            var type = semanticModel.GetTypeInfo(last.Type, cancellationToken).Type;
            if (type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Object)
                return true;
        }

        return false;
    }

    private static bool HasOverloadResolutionPriorityAttribute(MethodDeclarationSyntax method)
    {
        return method.AttributeLists
            .SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString().Contains("OverloadResolutionPriority"));
    }
}