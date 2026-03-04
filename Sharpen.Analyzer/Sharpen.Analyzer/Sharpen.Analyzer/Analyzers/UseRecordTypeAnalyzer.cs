using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRecordTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseRecordTypeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp9OrAbove(context.Compilation))
            return;

        var classDecl = (ClassDeclarationSyntax)context.Node;

        // Must be sealed.
        if (!classDecl.Modifiers.Any(SyntaxKind.SealedKeyword))
            return;

        // Must not have a base class.
        if (classDecl.BaseList != null && classDecl.BaseList.Types.Count > 0)
            return;

        // Must not be partial (conservative).
        if (classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return;

        // Only allow: auto-properties + constructors.
        foreach (var member in classDecl.Members)
        {
            if (member is PropertyDeclarationSyntax property)
            {
                if (!IsAutoProperty(property))
                    return;

                continue;
            }

            if (member is ConstructorDeclarationSyntax)
                continue;

            // Anything else (methods, fields, events, etc.) is considered behavioral/mutable.
            return;
        }

        // Must have at least one property.
        if (!classDecl.Members.OfType<PropertyDeclarationSyntax>().Any())
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseRecordTypeRule, classDecl.Identifier.GetLocation()));
    }

    private static bool IsAutoProperty(PropertyDeclarationSyntax property)
    {
        if (property.AccessorList == null)
            return false;

        foreach (var accessor in property.AccessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
                return false;
        }

        return true;
    }
}
