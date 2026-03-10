using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sharpen.Analyzer.Analyzers.CSharp9;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseTargetTypedNewAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseTargetTypedNewRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static bool IsCSharp9OrAbove(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.SyntaxTree.Options is not CSharpParseOptions parseOptions)
            return false;

        return parseOptions.LanguageVersion >= LanguageVersion.CSharp9;
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (!IsCSharp9OrAbove(context))
            return;

        if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
            return;

        // Don't touch arrays/implicit object creation (already target-typed) etc.
        if (objectCreation.Type == null)
            return;

        // 6.1 explicit-type local/field/property initializers
        if (objectCreation.Parent is EqualsValueClauseSyntax equalsValue)
        {
            var createdType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;

            if (equalsValue.Parent is VariableDeclaratorSyntax variableDeclarator
                && variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration
                && variableDeclaration.Type is not IdentifierNameSyntax { Identifier.ValueText: "var" })
            {
                // Explicit local declaration: T x = new T(...)
                // Only safe when the declared type is exactly the created type.
                // (e.g. don't suggest for: ICollection<object> x = new List<object>();)
                var declaredType = context.SemanticModel.GetTypeInfo(variableDeclaration.Type, context.CancellationToken).Type;
                if (declaredType != null && createdType != null && SymbolEqualityComparer.Default.Equals(declaredType, createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTargetTypedNewRule, objectCreation.GetLocation()));
                }

                return;
            }

            if (equalsValue.Parent is PropertyDeclarationSyntax propertyDeclaration)
            {
                // Property initializer: T P {get;} = new T(...)
                var declaredType = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type, context.CancellationToken).Type;
                if (declaredType != null && createdType != null && SymbolEqualityComparer.Default.Equals(declaredType, createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTargetTypedNewRule, objectCreation.GetLocation()));
                }

                return;
            }

            if (equalsValue.Parent is FieldDeclarationSyntax fieldDeclaration)
            {
                // Field initializer: T f = new T(...)
                var declaredType = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type, context.CancellationToken).Type;
                if (declaredType != null && createdType != null && SymbolEqualityComparer.Default.Equals(declaredType, createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTargetTypedNewRule, objectCreation.GetLocation()));
                }

                return;
            }
        }

        // 6.2 assignments/returns where target type is unambiguous
        if (objectCreation.Parent is AssignmentExpressionSyntax assignment && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
        {
            var leftType = context.SemanticModel.GetTypeInfo(assignment.Left, context.CancellationToken).Type;
            var rightType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;

            if (leftType != null && rightType != null && SymbolEqualityComparer.Default.Equals(leftType, rightType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTargetTypedNewRule, objectCreation.GetLocation()));
                return;
            }
        }

        if (objectCreation.Parent is ReturnStatementSyntax returnStatement)
        {
            var symbol = context.SemanticModel.GetEnclosingSymbol(returnStatement.SpanStart, context.CancellationToken);
            if (symbol is IMethodSymbol method)
            {
                var returnType = method.ReturnType;
                var createdType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;

                if (createdType != null && SymbolEqualityComparer.Default.Equals(returnType, createdType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rules.Rules.UseTargetTypedNewRule, objectCreation.GetLocation()));
                }
            }
        }
    }
}
