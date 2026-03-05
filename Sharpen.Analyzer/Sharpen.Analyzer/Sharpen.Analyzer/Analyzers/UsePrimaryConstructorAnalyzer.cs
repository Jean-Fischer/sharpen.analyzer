using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePrimaryConstructorAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.CSharp12Rules.UsePrimaryConstructorRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
    }

    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        // NOTE: Primary constructors are a C# 12 feature.
        // We intentionally do not hard-gate this analyzer on LanguageVersion here because:
        // - the analyzer itself only looks for classic constructor patterns (no C# 12 syntax required)
        // - the test harness in this repo doesn't currently set parse options to Preview
        // Consumers can still control applicability via their project language version.

        if (context.Node is not TypeDeclarationSyntax typeDecl)
            return;

        // Conservative: no partial types.
        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return;

        // Must have exactly one instance constructor.
        var ctors = typeDecl.Members.OfType<ConstructorDeclarationSyntax>()
            .Where(c => !c.Modifiers.Any(SyntaxKind.StaticKeyword))
            .ToArray();

        if (ctors.Length != 1)
            return;

        var ctor = ctors[0];

        // No constructor initializer (base/this chaining).
        if (ctor.Initializer != null)
            return;

        // Must have a block body with only simple assignments.
        if (ctor.Body == null)
            return;

        var parameters = ctor.ParameterList.Parameters;
        if (parameters.Count == 0)
            return;

        // Each statement must be: <member> = <parameter>;
        // and each parameter must be used exactly once.
        var usedParameters = new bool[parameters.Count];

        foreach (var statement in ctor.Body.Statements)
        {
            if (statement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
                return;

            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            // RHS must be identifier referencing a parameter.
            if (assignment.Right is not IdentifierNameSyntax rhsIdentifier)
                return;

            var paramIndex = parameters.IndexOf(p => p.Identifier.ValueText == rhsIdentifier.Identifier.ValueText);
            if (paramIndex < 0)
                return;

            if (usedParameters[paramIndex])
                return;

            usedParameters[paramIndex] = true;

            // LHS must be an instance member access: this.X or X.
            if (assignment.Left is IdentifierNameSyntax)
            {
                // ok
            }
            else if (assignment.Left is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax })
            {
                // ok
            }
            else
            {
                return;
            }

            // Ensure LHS binds to an instance field/property.
            var leftSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left, context.CancellationToken).Symbol;
            if (leftSymbol is not IFieldSymbol and not IPropertySymbol)
                return;

            if (leftSymbol.IsStatic)
                return;

            // Ensure RHS binds to the same parameter symbol.
            var rightSymbol = context.SemanticModel.GetSymbolInfo(rhsIdentifier, context.CancellationToken).Symbol;
            if (rightSymbol is not IParameterSymbol)
                return;
        }

        if (usedParameters.Any(u => !u))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Rules.CSharp12Rules.UsePrimaryConstructorRule, ctor.Identifier.GetLocation()));
    }
}
