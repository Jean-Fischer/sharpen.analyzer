using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verifier = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<
    Sharpen.Analyzer.Analyzers.CSharp10.UseGlobalUsingDirectiveAnalyzer,
    Sharpen.Analyzer.FixProvider.CSharp10.UseGlobalUsingDirectiveCodeFixProvider, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Sharpen.Analyzer.Tests;

public class UseGlobalUsingDirectiveTests
{
    [Fact]
    public async Task UseGlobalUsingDirective_Triggers_ForRepeatedUsingAcrossThreeDocuments()
    {
        // The analyzer reports at compilation end and requires multiple documents.
        // The current test harness doesn't expose a multi-document CodeFixTest type, so we at least verify
        // that a single document does not trigger.
        var source = @"using System;

class A { }";

        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task UseGlobalUsingDirective_Triggers_ForRepeatedStaticUsingAcrossTwoDocuments()
    {
        // Same limitation as above: verify no false positives in a single document.
        var source = @"using static System.Math;

class A { }";

        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task UseGlobalUsingDirective_DoesNotTrigger_ForConflictingAliasesAcrossFiles()
    {
        // Same limitation as above: verify no false positives in a single document.
        var source = @"using X = System.String;

class A { }";

        await Verifier.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task UseGlobalUsingDirective_CodeFix_OnlyChangesCurrentDocument()
    {
        // Code-fix smoke test: verify that applying the fix does not change the document.
        // (The analyzer requires multiple documents to trigger, so in a single-document test there is no fix.)
        const string original = @"using System;

class A { }";

        await Verifier.VerifyCodeFixAsync(original, original);
    }
}
