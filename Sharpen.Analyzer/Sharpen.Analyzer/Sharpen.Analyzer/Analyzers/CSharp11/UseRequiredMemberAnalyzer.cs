using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp11;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRequiredMemberAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp11Rules.UseRequiredMemberRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            if (!CSharpLanguageVersion.IsCSharp11OrAbove(compilationContext.Compilation))
                return;

            compilationContext.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        });
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;

        // Must be an auto-property.
        if (property.AccessorList == null)
            return;

        // Must have a setter or init.
        var accessors = property.AccessorList.Accessors;
        if (!accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)))
            return;

        // Must not have an initializer.
        if (property.Initializer != null)
            return;

        // Must not already be required.
        if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.RequiredKeyword)))
            return;

        // Exclude expression-bodied properties.
        if (property.ExpressionBody != null)
            return;

        var symbol = context.SemanticModel.GetDeclaredSymbol(property, context.CancellationToken);
        if (symbol == null)
            return;

        // Only properties on classes.
        if (symbol.ContainingType?.TypeKind != TypeKind.Class)
            return;

        // Exclude static.
        if (symbol.IsStatic)
            return;

        // Exclude interface implementations.
        if (symbol.ExplicitInterfaceImplementations.Length > 0)
            return;

        // Heuristic: public settable property.
        if (symbol.DeclaredAccessibility != Accessibility.Public)
            return;

        // Heuristic: non-nullable reference type.
        if (symbol.Type.IsReferenceType)
        {
            if (symbol.NullableAnnotation != NullableAnnotation.NotAnnotated)
                return;
        }
        else
        {
            // For now, keep it conservative: only suggest for reference types.
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp11Rules.UseRequiredMemberRule, property.Identifier.GetLocation()));
    }
}
