using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Sharpen.Analyzer.Tests.Infrastructure;

internal static class SafetyTestDocumentFactory
{
    public static async Task<(Document document, SyntaxTree syntaxTree, SemanticModel semanticModel)> CreateAsync(string source)
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var document = workspace.AddDocument(project.Id, "Test0.cs", SourceText.From(source));
        var syntaxTree = await document.GetSyntaxTreeAsync(CancellationToken.None)
                         ?? throw new InvalidOperationException("Unable to create syntax tree for test document.");
        var semanticModel = await document.GetSemanticModelAsync(CancellationToken.None)
                            ?? throw new InvalidOperationException("Unable to create semantic model for test document.");

        return (document, syntaxTree, semanticModel);
    }
}
