using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sharpen.Analyzer.Extensions;

public static class SyntaxNodeExtensions
{
    public static bool IsWithinLambdaOrAnonymousMethod(this SyntaxNode syntaxNode)
    {
        return syntaxNode.FirstAncestorOrSelf<AnonymousFunctionExpressionSyntax>() != null;
    }

    // Returning lists... mutable to be worse... I don't like it. Still, in this case, the best tradeoff
    // until we introduce some good both semantically correct and performant collection handling.

    extension(SyntaxNode syntaxNode)
    {
        public TNode? FirstAncestorOrSelfWithinEnclosingNode<TNode>(SyntaxNode enclosingNode, bool includeEnclosingNode = true) where TNode : SyntaxNode
        {
            // TODO-IG: We should assert here that the syntaxNode is really within the enclosingNode.
            //          Define in general how to do asserting.

            var currentNode = syntaxNode;
            while (currentNode != enclosingNode)
            {
                if (currentNode is TNode node) return node;
                currentNode = currentNode.Parent;
            }

            if (includeEnclosingNode && enclosingNode is TNode enclosingNodeAsTNode) return enclosingNodeAsTNode;

            return null;
        }

        /// <summary>
        ///     Returns true if the  <paramref name="syntaxNode" /> yields.
        ///     We say that a syntax node yields if it contains yield statements
        ///     that causes its first yieldable parent or self (e.g. method, local function,
        ///     property, etc.) to yield.
        /// </summary>
        public bool Yields()
        {
            // A yield statement only makes the *nearest yieldable ancestor* yield.
            // If the yield is inside a lambda/anonymous method or a local function nested within
            // the current syntaxNode, it must not be considered as making the current syntaxNode yield.
            return syntaxNode.DescendantNodes()
                .OfType<YieldStatementSyntax>()
                .Any(yieldStatement =>
                    IsNotWithinLambdaOrAnonymousMethodDifferentThanSyntaxNode(yieldStatement)
                    &&
                    IsNotWithinLocalFunctionDifferentThanSyntaxNode(yieldStatement)
                );

            bool IsNotWithinLambdaOrAnonymousMethodDifferentThanSyntaxNode(YieldStatementSyntax yieldStatement)
            {
                // If there is an anonymous function between the yield statement and the syntax node,
                // then the yield belongs to that anonymous function, not to the syntax node.
                var enclosingAnonymousFunction =
                    yieldStatement.FirstAncestorOrSelfWithinEnclosingNode<AnonymousFunctionExpressionSyntax>(syntaxNode,
                        includeEnclosingNode: false);
                return enclosingAnonymousFunction == null;
            }

            bool IsNotWithinLocalFunctionDifferentThanSyntaxNode(YieldStatementSyntax yieldStatement)
            {
                // If there is a local function between the yield statement and the syntax node,
                // then the yield belongs to that local function, not to the syntax node.
                var localFunction =
                    yieldStatement.FirstAncestorOrSelfWithinEnclosingNode<LocalFunctionStatementSyntax>(syntaxNode,
                        includeEnclosingNode: false);
                return localFunction == null;
            }
        }
    }

    // TODO-IG: Use these methods on the places where we now use nodes.Where(node => node.IsKind(...) ...).


    public static bool IsLeftSideOfAssignExpression(this SyntaxNode node)
    {
        return node.IsParentKind(SyntaxKind.SimpleAssignmentExpression) &&
               ((AssignmentExpressionSyntax)node.Parent).Left == node;
    }

    public static bool IsParentKind(this SyntaxNode? node, SyntaxKind kind)
    {
        return node.Parent.IsKind(kind);
    }

    extension(SyntaxNode node)
    {
        public bool IsObjectInitializerNamedAssignmentIdentifier(out SyntaxNode? initializedInstance)
        {
            initializedInstance = null;

            if (node is not IdentifierNameSyntax identifier ||
                !identifier.IsLeftSideOfAssignExpression() ||
                !identifier.Parent.IsParentKind(SyntaxKind.ObjectInitializerExpression))
            {
                return false;
            }

            if (identifier.Parent?.Parent is not InitializerExpressionSyntax objectInitializer)
            {
                return false;
            }

            // new C { P = 1 }
            if (objectInitializer.Parent is ObjectCreationExpressionSyntax objectCreation &&
                objectCreation.Parent is not AssignmentExpressionSyntax)
            {
                initializedInstance = objectCreation;
                return true;
            }

            // c = new C { P = 1 }
            if (objectInitializer.Parent is not ObjectCreationExpressionSyntax { Parent: AssignmentExpressionSyntax assignmentExpression } objectCreationOnRight ||
                !assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression) ||
                assignmentExpression.Right != objectCreationOnRight)
            {
                return false;
            }

            initializedInstance = assignmentExpression.Left;
            return true;

        }
    }
}