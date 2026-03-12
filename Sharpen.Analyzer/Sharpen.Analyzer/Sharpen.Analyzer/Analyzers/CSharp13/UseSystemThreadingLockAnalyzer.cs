using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Rules;

namespace Sharpen.Analyzer.Analyzers.CSharp13;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseSystemThreadingLockAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(CSharp13Rules.UseSystemThreadingLockRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

        // Only consider private fields.
        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.PrivateKeyword))
            return;

        // Only consider `object` fields.
        var fieldType = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type, context.CancellationToken).Type;
        if (fieldType is null || fieldType.SpecialType != SpecialType.System_Object)
            return;

        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
            if (fieldSymbol is null)
                continue;

            // Dedicated sync object: used only as lock target.
            if (!IsUsedOnlyInLockStatements(context, fieldSymbol))
                continue;

            context.ReportDiagnostic(Diagnostic.Create(
                CSharp13Rules.UseSystemThreadingLockRule,
                variable.Identifier.GetLocation()));
        }
    }

    private static bool IsUsedOnlyInLockStatements(SyntaxNodeAnalysisContext context, IFieldSymbol fieldSymbol)
    {
        var root = context.Node.SyntaxTree.GetRoot(context.CancellationToken);

        // Find all identifier usages of the field in the syntax tree.
        // Conservative: if we can't prove all usages are lock targets, do not report.
        var identifiers = root.DescendantNodes().OfType<IdentifierNameSyntax>();

        var anyUsage = false;

        foreach (var identifier in identifiers)
        {
            if (identifier.Identifier.ValueText != fieldSymbol.Name)
                continue;

            var symbol = context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol;
            if (!SymbolEqualityComparer.Default.Equals(symbol, fieldSymbol))
                continue;

            anyUsage = true;

            // Must be `lock(<identifier>)`.
            if (identifier.Parent is not LockStatementSyntax lockStatement)
                return false;

            if (!ReferenceEquals(lockStatement.Expression, identifier))
                return false;
        }

        return anyUsage;
    }
}
