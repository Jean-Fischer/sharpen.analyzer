## 1. Baseline and shared resolution helper

- [x] 1.1 Identify the current analyzer behavior and confirm it uses [`EquivalentAsynchronousMethodFinder.EquivalentAsynchronousCandidateExistsFor()`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/Common/EquivalentAsynchronousMethodFinder.cs:141) with `CallerMustBeAsync` and yielding irrelevant
- [x] 1.2 Add a shared helper (new or refactor existing) that resolves the async equivalent `IMethodSymbol` for a given invocation using the same rules as the finder (two-type search + `LookupSymbols` at invocation position)
- [x] 1.3 Ensure the helper applies the same equivalence predicate as the finder (return type rules + parameter type/name rules + optional trailing `CancellationToken`)

## 2. Code fix: invocation rewrite

- [x] 2.1 Update [`AwaitEquivalentAsynchronousMethodCodeFixProvider`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer/FixProvider/AwaitEquivalentAsynchronousMethodCodeFixProvider.cs:1) to use the shared resolver instead of `Name + Async` member scanning
- [x] 2.2 Implement safe rewrite for member access and identifier invocations (preserve receiver and argument list; replace invoked name)
- [x] 2.3 Preserve leading/trailing trivia when replacing the invocation node

## 3. Code fix: await insertion rules

- [x] 3.1 Detect and avoid double-await when the invocation is already within an `await` expression
- [x] 3.2 Add `await` for expression-statement invocations inside async methods/local functions
- [x] 3.3 Add `await` for assignment RHS invocations inside async methods/local functions
- [x] 3.4 Handle return statements by rewriting `return X();` to `return await XAsync();` in async methods
- [x] 3.5 Add guardrails to skip code fix when the invocation context cannot safely accept `await` (to avoid producing invalid syntax)

## 4. Tests

- [x] 4.1 Keep and stabilize the existing happy-path test in [`AwaitEquivalentAsynchronousMethodCodeFixTests`](Sharpen.Analyzer/Sharpen.Analyzer/Sharpen.Analyzer.Tests/AwaitEquivalentAsynchronousMethodCodeFixTests.cs:1)
- [x] 4.2 Add test: already-awaited invocation becomes single-await async equivalent
- [x] 4.3 Add test: assignment RHS is rewritten to `await <async-invocation>`
- [x] 4.4 Add test: return statement is rewritten to `return await <async-invocation>`
- [x] 4.5 Add test: non-async caller produces no diagnostic
- [x] 4.6 Add test: extension method equivalent is resolved and fixed correctly (reduced extension method case)
- [x] 4.7 Add test: ignored methods (e.g., EF Core `DbSet.Add`) do not produce diagnostics

## 5. Verification

- [x] 5.1 Run analyzer + code fix tests and ensure all pass
- [x] 5.2 Manually sanity-check a few representative samples (instance, static, extension) to confirm the code fix matches analyzer resolution
