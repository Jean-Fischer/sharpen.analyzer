using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp10;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRecordStructAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp10Rules.UseRecordStructRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
    {
        if (!CSharpLanguageVersion.IsCSharp10OrAbove(context.Compilation))
            return;

        var decl = (StructDeclarationSyntax)context.Node;

        // Skip if already record struct.
        if (decl.Modifiers.Any(m => m.IsKind(SyntaxKind.RecordKeyword)))
            return;

        // Conservative heuristics: public struct with only public fields OR get-only auto-properties,
        // optional constructors, and no methods/events/operators/etc.
        if (!HasSupportedDeclarationShape(decl))
            return;

        var nonCtorMembers = decl.Members.Where(m => m is not ConstructorDeclarationSyntax).ToList();
        if (!ContainsOnlyFieldAndPropertyMembers(nonCtorMembers))
            return;

        if (!AllFieldsAreSupported(nonCtorMembers) || !AllPropertiesAreSupported(nonCtorMembers))
            return;

        context.ReportDiagnostic(Diagnostic.Create(CSharp10Rules.UseRecordStructRule, decl.Identifier.GetLocation()));
    }

    private static bool HasSupportedDeclarationShape(StructDeclarationSyntax decl)
    {
        return decl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))
               && decl.BaseList == null
               && decl.Members.Any()
               && !HasDisallowedMembers(decl);
    }

    private static bool HasDisallowedMembers(StructDeclarationSyntax decl)
    {
        return decl.Members.Any(m => m is MethodDeclarationSyntax
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
    }

    private static bool ContainsOnlyFieldAndPropertyMembers(System.Collections.Generic.IEnumerable<MemberDeclarationSyntax> members)
    {
        return members.All(m => m is FieldDeclarationSyntax or PropertyDeclarationSyntax);
    }

    private static bool AllFieldsAreSupported(System.Collections.Generic.IEnumerable<MemberDeclarationSyntax> members)
    {
        return members.OfType<FieldDeclarationSyntax>().All(field =>
            field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))
            && !field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword)));
    }

    private static bool AllPropertiesAreSupported(System.Collections.Generic.IEnumerable<MemberDeclarationSyntax> members)
    {
        return members.OfType<PropertyDeclarationSyntax>().All(IsSupportedProperty);
    }

    private static bool IsSupportedProperty(PropertyDeclarationSyntax prop)
    {
        if (!prop.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)) || prop.AccessorList == null)
            return false;

        var accessors = prop.AccessorList.Accessors;
        return accessors.Count == 1
               && accessors[0].IsKind(SyntaxKind.GetAccessorDeclaration)
               && accessors[0].Body == null
               && accessors[0].ExpressionBody == null;
    }
}
