using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Sharpen.Analyzer.Extensions;

namespace Sharpen.Analyzer.Common;
// TODO-IG: Refactor this. The class heavily violates the single responsibility principle.
//          It does two heavy things at the moment, searches for the equivalent async methods,
//          but also checks the "environment" to see if it fits requirements of just a one
//          specific client. Bad.

/// <summary>
///
/// </summary>
/// <remarks>
///     We can have different finders. Hardcoded one, one based on dependencies etc.
///     This base class encapsulates the common and exact search logic.
///     The derived classes are responsible for the heuristic part of the search.
/// </remarks>
internal abstract class EquivalentAsynchronousMethodFinder
{
    public enum CallerAsyncStatus
    {
        CallerMustBeAsync,
        CallerMustNotBeAsync
    }

    public enum CallerYieldingStatus
    {
        Irrelevant,
        CallerMustYield,
        CallerMustNotYield
    }

    /// <summary>
    ///     Returns true if an equivalent asynchronous method of the method used in
    ///     the <paramref name="invocation" /> exists and is at the same time a
    ///     potential candidate to be used exactly in that particular <paramref name="invocation" />.
    /// </summary>
    /// <remarks>
    ///     This method does not only check if an equivalent asynchronous method
    ///     exists. In addition, it runs additional checks to see if it make sense
    ///     to call such existing asynchronous equivalent in the particular <paramref name="invocation" />.
    ///     For example, the suggestion to await asynchronous equivalent
    ///     makes sense only if the enclosing method
    ///     within which the invocation happens can be turned into async method.
    ///     For example, if the enclosing method is an interface implementation or an override
    ///     method of an interface or base class that cannot be changed, than it cannot be
    ///     turned into async method and thus the suggestion makes no sense.
    /// </remarks>
    public bool EquivalentAsynchronousCandidateExistsFor(InvocationExpressionSyntax invocation,
        SemanticModel semanticModel, CallerAsyncStatus callerAsyncStatus, CallerYieldingStatus callerYieldingStatus)
    {
        if (!InvokedMethodPotentiallyHasAsynchronousEquivalent(invocation)) return false;

        if (!(semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol method)) return false;

        if (EquivalentAsynchronousMethodMetadata.IsIgnoredMethod(method)) return false;

        // So far we do not suggest turning lambdas and anonymous methods
        // into async. So we will at the moment just ignore that case.
        // TODO: Support suggestion for lambdas and anonymous methods.
        if (invocation.IsWithinLambdaOrAnonymousMethod()) return false;

        // If type authors invoke the synchronous method
        // within the implementation of its containing type
        // we assume that they exactly know what they are doing.
        // They for sure want to call exactly that method on
        // that particular place in code. We are 100% sure that
        // they do not want to call its async equivalent.
        if (MethodIsInvokedWithinItsContainingType()) return false;

        // The suggestions make sense only if the whole calling chain
        // already is or can be made async by utilizing the async keyword.
        if (!MethodIsInvokedWithinACallerNodeThatCanBeMarkedAsAsync()) return false;

        // Caller is either a method or a local function.
        if (!TryGetEnclosingLocalFunctionOrMethod(out var callerSymbol, out var callerSyntaxNode)) return false;

        if (callerAsyncStatus == CallerAsyncStatus.CallerMustNotBeAsync)
        {
            if (callerSymbol.IsAsync) return false;

            if (!CallerCanBeMadeAsync()) return false;
        }
        else // Caller must be async.
        {
            if (!callerSymbol.IsAsync) return false;
        }

        if (callerYieldingStatus != CallerYieldingStatus.Irrelevant)
        {
            var callerYields = callerSyntaxNode.Yields();
            if (callerYields && callerYieldingStatus != CallerYieldingStatus.CallerMustYield) return false;
            if (!callerYields && callerYieldingStatus != CallerYieldingStatus.CallerMustNotYield) return false;
        }

        // We can have the following situations:
        // 1. someObject.SomeInstanceMethod()
        // 2. someObject.SomeExtensionMethod()
        // 3. SomeType.SomeStaticMethod()
        // 4. <build in type keyword>.SomeStaticMethod()
        // 5. SomeInstanceMethod();
        // 6. SomeStaticMethod();
        // 7. this.SomeInstanceMethod();
        // A potential asynchronous equivalent can be defined on
        // the same type on which the synchronous method is defined.
        // But if the synchronous method is itself an extension method,
        // the asynchronous equivalent could be defined on the type
        // of the object on which the method is called.
        // And other way around, if the synchronous method is an instance
        // method, the asynchronous equivalent could be an extension method
        // on an arbitrary type that extends the type of the instance ;-)
        // Long story short, the search for the asynchronous equivalent
        // has to check both the containing type of the synchronous method
        // and all the possible methods that can be called on the instance
        // on which the synchronous method is called (if there are such).

        // Let's check the method containing type first.
        if (EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
                semanticModel,
                method.ContainingType,
                method,
                invocation))
        {
            return true;
        }

        // Let's now check the type on which the method is called,
        // if there is such.
        var calledOnType = GetCalledOnType();
        if (calledOnType == null || SymbolEqualityComparer.Default.Equals(calledOnType, method.ContainingType)) return false;

        return EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
            semanticModel,
            calledOnType,
            method,
            invocation);

        bool MethodIsInvokedWithinACallerNodeThatCanBeMarkedAsAsync()
        {
            // We do not want to have suggestions on methods with async
            // equivalents that are called in constructors, properties,
            // destructors, etc. because those, with a good reason!,
            // cannot be made async in C#. The suggestion makes sense
            // only if the whole calling chain already is or can be made
            // async.

            // The only two C# elements that can be made async are methods
            // and local functions. Local functions can be nested in e.g.
            // properties. In that case there is no sense of making them
            // async.
            // Thus, ultimately, we have to see if the invocation is done
            // within a method.

            // A MethodDeclarationSyntax cannot be nested within an
            // another MethodDeclarationSyntax. Therefore we can just search
            // for the first parent of type MethodDeclarationSyntax.
            return invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>() != null;

            // (Afterthought. We could have a situation that someone has an async
            // local function within a property and within the function calls a
            // method that has an async equivalent. We will simply ignore this.
            // Having async local functions in properties, constructors, etc. makes
            // zero sense.)
        }

        bool TryGetEnclosingLocalFunctionOrMethod(out IMethodSymbol pCallerSymbol, out SyntaxNode pcallerSyntaxNode)
        {
            // Of course, first we have to check if the invocation happens within a local function.
            // The declaration symbol of a local function implements IMethodSymbol so the cast is safe.
            var enclosingLocalFunction = invocation.FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
            if (enclosingLocalFunction != null)
            {
                pCallerSymbol = semanticModel.GetDeclaredSymbol(enclosingLocalFunction)!;
                pcallerSyntaxNode = enclosingLocalFunction;
                return true;
            }

            var enclosingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (enclosingMethod != null)
            {
                pCallerSymbol = semanticModel.GetDeclaredSymbol(enclosingMethod)!;
                pcallerSyntaxNode = enclosingMethod;
                return true;
            }

            pCallerSymbol = null!;
            pcallerSyntaxNode = null!;
            return false;
        }

        bool CallerCanBeMadeAsync()
        {
            // In this method we are only checking if adding the "async" keyword
            // and changing the return type to e.g. Task<Something> will not break
            // existing outer contracts posed on the method, e.g. if the method
            // is an interface implementation we cannot change its return type.

            // We do not check if its internal implementation can suppress us from
            // making it async. E.g. if it yields and we are in C# less then 8.0
            // (no async streams). Other checks in this class are responsible
            // for covering those constraints.

            // If we have an enclosing local function, it can neither override base methods
            // nor implement interfaces. So it has no restrictions of that kind.
            // And for the moment, we will assume that there is no an equivalent asynchronous
            // local function in the same scope. I can imagine a case where such and equivalent
            // could exist, but this is corner case. If it ever happens, let's have a false
            // positive and hopefully an issue filled :-)

            // Long story short, if we have a local function then yes, it can always be made async.
            if (callerSymbol.MethodKind == MethodKind.LocalFunction)
                return true;

            return CallerMethodDoesNotOverrideNonChangeableBaseClassMethod() &&
                   CallerMethodDoesNotImplementNonChangeableInterfaceMethod() &&
                   CallerMethodDoesNotAlreadyHaveAsynchronousEquivalent();

            bool CallerMethodDoesNotOverrideNonChangeableBaseClassMethod()
            {
                if (!callerSymbol.IsOverride) return true;

                return callerSymbol.OverriddenMethod?
                    .ContainingType?
                    .Locations.All(location => location.IsInSource) == true;
            }

            bool CallerMethodDoesNotImplementNonChangeableInterfaceMethod()
            {
                // If the enclosing method implements an interface method
                // we have to see if that interface can be changed.
                // Since it could implement more then one interface, we have
                // to check of all of them can be changed.
                // (Changed means made async.)
                // If they cannot, means if they are referenced from an assembly
                // and not defined in code, we assume that the enclosing
                // method implements a non-changeable interface method.
                return callerSymbol.GetImplementedInterfaceMethods()
                    .All(interfaceMethod => interfaceMethod
                        .ContainingType?.Locations.All(location => location.IsInSource) == true);
            }

            bool CallerMethodDoesNotAlreadyHaveAsynchronousEquivalent()
            {
                return !EquivalentAsynchronousMethodLookup.TypeContainsAsynchronousEquivalent(
                    semanticModel,
                    callerSymbol.ContainingType,
                    callerSymbol);
            }
        }

        bool MethodIsInvokedWithinItsContainingType()
        {
            var invokedInType = invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>();

            // This should never happen. It means we have some issue in the
            // syntax tree. If that's the case, just cancel any further analysis
            // by stating that the method is called withing its containing type.
            if (invokedInType == null) return true;

            return SymbolEqualityComparer.Default.Equals(method.ContainingType, semanticModel.GetDeclaredSymbol(invokedInType));
        }

        INamedTypeSymbol? GetCalledOnType()
        {
            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return null;

            return semanticModel.GetTypeInfo(memberAccess.Expression).Type as INamedTypeSymbol;
        }
    }

    protected abstract bool InvokedMethodPotentiallyHasAsynchronousEquivalent(InvocationExpressionSyntax invocation);

}
