using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp12;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseInlineArrayAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp12Rules.UseInlineArrayRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeStruct, SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeStruct(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not StructDeclarationSyntax @struct)
            return;

        // If already has InlineArray attribute, don't report.
        if (@struct.AttributeLists.SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString().EndsWith("InlineArray", StringComparison.Ordinal)))
            return;

        // Must be a plain struct with only fields.
        if (@struct.Members.Any(m => m is not FieldDeclarationSyntax))
            return;

        // No attributes (keep conservative; layout/other attributes can affect semantics).
        if (@struct.AttributeLists.Count > 0)
            return;

        var fields = @struct.Members.OfType<FieldDeclarationSyntax>().ToArray();
        if (fields.Length == 0)
            return;

        // Only allow single-variable fields.
        if (fields.Any(f => f.Declaration.Variables.Count != 1))
            return;

        // Determine element type and count.
        var firstField = fields[0];
        var elementType = context.SemanticModel.GetTypeInfo(firstField.Declaration.Type).Type;
        if (elementType is null)
            return;

        // Accept either:
        //  - a single field named _element0
        //  - a sequence _element0.._element{N-1}
        var names = fields.Select(f => f.Declaration.Variables[0].Identifier.ValueText).ToArray();

        if (names.Length == 1)
        {
            if (names[0] != "_element0")
                return;

            context.ReportDiagnostic(Diagnostic.Create(CSharp12Rules.UseInlineArrayRule,
                @struct.Identifier.GetLocation(), 1));
            return;
        }

        for (var i = 0; i < names.Length; i++)
        {
            if (names[i] != $"_element{i}")
                return;

            var t = context.SemanticModel.GetTypeInfo(fields[i].Declaration.Type).Type;
            if (!SymbolEqualityComparer.Default.Equals(t, elementType))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(CSharp12Rules.UseInlineArrayRule, @struct.Identifier.GetLocation(),
            names.Length));
    }
}