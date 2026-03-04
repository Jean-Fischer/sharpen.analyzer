using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRecordStructAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp10Rules.UseRecordStructRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
    {
        if (!Common.CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var decl = (StructDeclarationSyntax)context.Node;

        // Skip if already record struct.
        if (decl.Modifiers.Any(m => m.IsKind(SyntaxKind.RecordKeyword)))
            return;

        // Conservative heuristics: public struct with only public fields OR get-only auto-properties,
        // optional constructors, and no methods/events/operators/etc.
        if (!decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
            return;

        if (decl.BaseList != null)
            return;

        if (decl.Members.Count == 0)
            return;

        var hasDisallowedMember = decl.Members.Any(m => m is MethodDeclarationSyntax
                                                        or EventDeclarationSyntax
                                                        or EventFieldDeclarationSyntax
                                                        or OperatorDeclarationSyntax
                                                        or ConversionOperatorDeclarationSyntax
                                                        or IndexerDeclarationSyntax
                                                        or DelegateDeclarationSyntax
                                                        or EnumDeclarationSyntax
                                                        or ClassDeclarationSyntax
                                                        or StructDeclarationSyntax
                                                        or InterfaceDeclarationSyntax);
        if (hasDisallowedMember)
            return;

        // Allow constructors.
        var nonCtorMembers = decl.Members.Where(m => m is not ConstructorDeclarationSyntax).ToList();

        // Must be composed of fields and/or properties.
        if (nonCtorMembers.Any(m => m is not FieldDeclarationSyntax && m is not PropertyDeclarationSyntax))
            return;

        // Fields must be public and not const.
        foreach (var field in nonCtorMembers.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return;

            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)))
                return;
        }

        // Properties must be public get-only auto-properties.
        foreach (var prop in nonCtorMembers.OfType<PropertyDeclarationSyntax>())
        {
            if (!prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                return;

            if (prop.AccessorList == null)
                return;

            var accessors = prop.AccessorList.Accessors;
            if (accessors.Count != 1)
                return;

            var getter = accessors[0];
            if (!getter.IsKind(SyntaxKind.GetAccessorDeclaration))
                return;

            // Auto-property: no body/expression body.
            if (getter.Body != null || getter.ExpressionBody != null)
                return;

            // No initializer restrictions.
        }

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp10Rules.UseRecordStructRule, decl.Identifier.GetLocation()));
    }
}
