using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseInitOnlySetterAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseInitOnlySetterRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp9OrAbove(context.Compilation))
            return;

        var property = (PropertyDeclarationSyntax)context.Node;

        // Only auto-properties.
        if (property.AccessorList == null)
            return;

        var accessors = property.AccessorList.Accessors;
        if (accessors.Count == 0)
            return;

        var setAccessor = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (setAccessor == null)
            return;

        // Must be an auto-property setter.
        if (setAccessor.Body != null || setAccessor.ExpressionBody != null)
            return;

        // Must be `private set;`.
        if (!setAccessor.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            return;

        // Must have a getter.
        if (!accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)))
            return;

        // Exclude interface implementations.
        var symbol = context.SemanticModel.GetDeclaredSymbol(property, context.CancellationToken);
        if (symbol == null)
            return;

        if (symbol.ExplicitInterfaceImplementations.Length > 0)
            return;

        // Exclude abstract properties.
        if (symbol.IsAbstract)
            return;

        // Conservative: ensure the property is not assigned outside constructors.
        // We only look within the containing type and only for direct assignments to this property.
        if (property.Parent is not TypeDeclarationSyntax containingType)
            return;

        if (IsAssignedOutsideConstructor(context, symbol, containingType))
            return;

        context.ReportDiagnostic(
            Diagnostic.Create(Rules.Rules.UseInitOnlySetterRule, property.Identifier.GetLocation()));
    }

    private static bool IsAssignedOutsideConstructor(
        SyntaxNodeAnalysisContext context,
        IPropertySymbol propertySymbol,
        TypeDeclarationSyntax containingType)
    {
        return (from assignment in containingType.DescendantNodes().OfType<AssignmentExpressionSyntax>() let leftSymbol = CSharpExtensions.GetSymbolInfo(context.SemanticModel, (ExpressionSyntax)assignment.Left, context.CancellationToken).Symbol where SymbolEqualityComparer.Default.Equals(leftSymbol, propertySymbol) select assignment).Any(assignment => assignment.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() == null);
    }
}