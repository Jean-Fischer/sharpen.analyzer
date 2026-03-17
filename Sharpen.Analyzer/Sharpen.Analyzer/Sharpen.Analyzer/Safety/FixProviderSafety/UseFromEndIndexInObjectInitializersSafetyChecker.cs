using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Safety.FixProviderSafety;

public sealed class UseFromEndIndexInObjectInitializersSafetyChecker : IFixProviderSafetyChecker
{
    public FixProviderSafetyResult IsSafe(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        Diagnostic diagnostic,
        CancellationToken cancellationToken = default)
    {
        if (diagnostic is null)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "diagnostic-null");

        if (syntaxTree?.Options.Language != LanguageNames.CSharp)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "not-csharp");

        var root = syntaxTree.GetRoot(cancellationToken);
        var indexExpressionNode = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        if (indexExpressionNode is not ExpressionSyntax indexExpression)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "index-expression-not-found");

        // In object initializers, the indexer assignment uses ImplicitElementAccessSyntax.
        if (indexExpression.Parent is not ArgumentSyntax
            {
                Parent: BracketedArgumentListSyntax { Parent: ImplicitElementAccessSyntax }
            })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "implicit-element-access-not-found");

        // Conservative: only allow when the `.Length` target is an array.
        // This ensures `^` indexing is supported and semantics are well-defined.
        if (indexExpression is not BinaryExpressionSyntax { Left: MemberAccessExpressionSyntax memberAccess })
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "length-minus-one-not-found");

        var lengthTargetType = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
        if (lengthTargetType is not IArrayTypeSymbol)
            return FixProviderSafetyResult.Unsafe(FixProviderSafetyStage.Local, "length-target-not-array");

        return FixProviderSafetyResult.Safe();
    }
}