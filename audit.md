📋 Sharpen.Analyzer – Code Fix Reliability Audit
🎯 Objective

Improve the safety, predictability, and robustness of code fix providers by:

Preventing unintended code modifications
Ensuring fixes are stable and repeatable
Increasing confidence in real-world usage (large / messy codebases)
🚨 Executive Summary (What’s wrong today)

Current test coverage validates:

✅ Fix correctness in controlled scenarios
❌ Fix scope control (what should NOT change)
❌ Fix stability (idempotency, fix-all behavior)
❌ Real-world robustness

Main Risk

Code fixes may:

Modify unintended code regions
Apply multiple times incorrectly
Break compilation in edge cases
Behave inconsistently under FixAll
🧠 Root Cause (Systemic)
1. Fix providers are likely too broad in node selection

Typical problematic pattern:

var nodes = root.DescendantNodes()
    .OfType<InvocationExpressionSyntax>()
    .Where(...);

➡ This ignores the diagnostic span and may match unintended nodes.

2. Tests validate output, not transformation correctness

Current tests:

await VerifyCodeFixAsync(before, after);

Missing:

What changed?
What should NOT have changed?
3. No validation of fix stability

Missing guarantees:

Fix is idempotent
FixAll behaves correctly
Fix does not introduce compilation errors
🛠️ Required Improvements
✅ 1. Enforce Diagnostic-Scoped Fixing
🔧 Rule

All fixes MUST be anchored to:

context.Span
✅ Correct pattern
var root = await document.GetSyntaxRootAsync(cancellationToken);
var node = root.FindNode(context.Span);

var target = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

🚫 Avoid:

root.DescendantNodes()
✅ 2. Introduce Idempotency Tests (MANDATORY)
🔧 Requirement

Applying a fix twice must produce the same result.

✅ Example
async Task VerifyIdempotent(string input)
{
    var fixedOnce = await ApplyFixAsync(input);
    var fixedTwice = await ApplyFixAsync(fixedOnce);

    Assert.Equal(fixedOnce, fixedTwice);
}
🎯 Detects
Cascading fixes
Overlapping transformations
Unstable rewrites
✅ 3. Add Multi-Diagnostic Scenario Tests
🔧 Requirement

Test multiple occurrences in a single file.

✅ Example
class Test
{
    void M()
    {
        Problem(); // should fix
        Problem(); // should fix
        Safe();    // MUST NOT change
    }
}
Assertions:
Only intended nodes are modified
Safe code remains unchanged
✅ 4. Add FixAll Coverage (CRITICAL)
🔧 Requirement

Test:

FixAll in Document
FixAll in Project
✅ Example
await VerifyCodeFixAsync(
    before,
    after,
    numberOfFixAllIterations: 1);
🎯 Detects
Batch processing issues
Inconsistent behavior across multiple diagnostics
⚠️ 5. Introduce Scope Validation (HIGH PRIORITY)
🔧 Requirement

Ensure fixes only modify intended regions.

Option A (Quick Win)

Count occurrences:

Assert.Equal(
    expectedCount,
    CountOccurrences(after, "Problem"));
Option B (Recommended)

Compare syntax trees:

var beforeTree = Parse(before);
var afterTree = Parse(after);

AssertOnlyExpectedNodeChanged(beforeTree, afterTree);
⚠️ 6. Add Compilation Validation
🔧 Requirement

Fixed code must compile cleanly.

✅ Example
var compilation = await GetCompilationAsync(fixedCode);

Assert.DoesNotContain(
    compilation.GetDiagnostics(),
    d => d.Severity == DiagnosticSeverity.Error);
⚠️ 7. Add Real-World Scenario Tests
🔧 Requirement

Introduce large, messy test cases.

Include:
Nested classes
Generics
LINQ
Mixed valid/invalid patterns
Irregular formatting
🔬 Code Fix Implementation Guidelines
✅ Use DocumentEditor instead of raw ReplaceNode
var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
editor.ReplaceNode(oldNode, newNode);
✅ Preserve trivia (formatting/comments)
newNode = newNode.WithTriviaFrom(oldNode);
✅ Use Semantic Model when needed

Avoid syntax-only assumptions.

✅ Narrow transformations

Only modify:

Exact node from diagnostic
Or its direct parent
❌ Anti-Patterns to Eliminate
🚫 Broad node scanning
root.DescendantNodes().OfType<T>()
🚫 Multiple replacements in single fix

Unless explicitly intended.

🚫 Ignoring diagnostic span
🚫 Non-idempotent transformations
🧪 Suggested Test Infrastructure Additions
Helper: ApplyFixAsync

Create reusable helper:

Task<string> ApplyFixAsync(string input);
Helper: Idempotency assertion
Task AssertIdempotent(string input);
Helper: Compilation check
Task AssertCompiles(string code);
🚀 Implementation Roadmap
Phase 1 (Immediate – High ROI)
 Add idempotency tests
 Add multi-diagnostic tests
 Add FixAll tests
Phase 2 (Stability)
 Add scope validation
 Add compilation checks
Phase 3 (Robustness)
 Add large scenario tests
 Introduce mutation/fuzz testing (optional)
📊 Expected Outcomes

After implementation:

✅ Fixes only affect intended code
✅ No accidental transformations
✅ Stable FixAll behavior
✅ No regression in large files
✅ Increased developer trust in analyzers

🧭 Final Note

This is not about adding more tests—it’s about changing what correctness means:

From: “does the fix work?”
To: “does the fix work AND only do exactly what it should?”

If you want next step, I can:

Audit one specific fix provider and rewrite it with your team’s patterns
Or design a shared test base class your devs can reuse across all analyzers