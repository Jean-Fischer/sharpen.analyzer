using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Sharpen.Analyzer.Common;

namespace Sharpen.Analyzer.Analyzers.CSharp6;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseNameofExpressionForThrowingArgumentExceptionsAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.Rules.UseNameofExpressionForThrowingArgumentExceptionsRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
    }

    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        var throwStatement = (ThrowStatementSyntax)context.Node;

        if (throwStatement.Expression is not ObjectCreationExpressionSyntax objectCreation) return;

        if (objectCreation.ArgumentList is null) return;

        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        if (semanticModel.GetTypeInfo(objectCreation, cancellationToken).Type is not INamedTypeSymbol createdType) return;

        if (!IsSupportedArgumentExceptionType(createdType)) return;

        if (!TryGetParameterNameArgumentIndex(objectCreation, semanticModel, cancellationToken,
                out var paramNameArgIndex)) return;

        if (paramNameArgIndex < 0 || paramNameArgIndex >= objectCreation.ArgumentList.Arguments.Count) return;

        var paramNameArgument = objectCreation.ArgumentList.Arguments[paramNameArgIndex];
        var paramNameExpression = paramNameArgument.Expression;

        if (CSharp6SyntaxHelpers.IsNameofExpression(paramNameExpression)) return;

        if (!CSharp6SyntaxHelpers.TryGetStringLiteralValue(paramNameExpression, out var paramName)) return;

        if (!IsInScopeParameterName(paramName, throwStatement, semanticModel, cancellationToken)) return;

        context.ReportDiagnostic(
            Diagnostic.Create(
                Rules.Rules.UseNameofExpressionForThrowingArgumentExceptionsRule,
                paramNameExpression.GetLocation()));
    }

    private static bool IsSupportedArgumentExceptionType(INamedTypeSymbol createdType)
    {
        // Keep this minimal and explicit.
        // We only support the common BCL argument exception types.
        var name = createdType.Name;
        if (name is not (
            nameof(ArgumentException) or
            nameof(ArgumentNullException) or
            nameof(ArgumentOutOfRangeException)))
            return false;

        // Ensure it's System.*
        return createdType.ContainingNamespace?.ToDisplayString() == "System";
    }

    private static bool TryGetParameterNameArgumentIndex(ObjectCreationExpressionSyntax objectCreation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken,
        out int index)
    {
        index = -1;

        // Prefer the actual constructor symbol to determine which argument is the paramName.
        var ctor = semanticModel.GetSymbolInfo(objectCreation, cancellationToken).Symbol as IMethodSymbol;

        // Find the parameter named "paramName".
        // This matches the BCL constructors for ArgumentException/ArgumentNullException/ArgumentOutOfRangeException.
        var paramNameParam = ctor?.Parameters.FirstOrDefault(p => p.Name == "paramName");
        if (paramNameParam is null) return false;

        // Map the constructor parameter to the argument index.
        // Handle named arguments and positional arguments.
        var args = objectCreation.ArgumentList?.Arguments;
        if (args is null) return false;

        // Named argument: paramName: "p"
        for (var i = 0; i < args.Value.Count; i++)
        {
            var arg = args.Value[i];
            if (arg.NameColon is null || arg.NameColon.Name.Identifier.ValueText != "paramName") continue;
            index = i;
            return true;
        }

        // Positional argument: use the parameter ordinal.
        index = paramNameParam.Ordinal;
        return true;
    }

    private static bool IsInScopeParameterName(
        string paramName,
        ThrowStatementSyntax throwStatement,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        // Find the nearest enclosing symbol that can have parameters.
        // This covers methods, constructors, local functions, and lambdas.
        var enclosingSymbol = semanticModel.GetEnclosingSymbol(throwStatement.SpanStart, cancellationToken);
        if (enclosingSymbol is null) return false;

        var parameters = Enumerable.Empty<IParameterSymbol>();

        if (enclosingSymbol is IMethodSymbol methodSymbol)
            parameters = methodSymbol.Parameters;
        else if (enclosingSymbol is IPropertySymbol propertySymbol)
            // Indexer accessors can throw; include indexer parameters.
            parameters = propertySymbol.Parameters;

        return parameters.Any(p => p.Name == paramName);
    }
}