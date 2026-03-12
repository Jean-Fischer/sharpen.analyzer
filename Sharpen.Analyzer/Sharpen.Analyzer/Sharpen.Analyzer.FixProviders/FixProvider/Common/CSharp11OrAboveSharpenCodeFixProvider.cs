using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Sharpen.Analyzer.FixProvider.Common;

public abstract class CSharp11OrAboveSharpenCodeFixProvider : SharpenCodeFixProvider
{
    protected sealed override async Task<bool> ShouldRegisterFixesAsync(Document document, CancellationToken ct)
    {
        var compilation = await document.Project.GetCompilationAsync(ct).ConfigureAwait(false);
        return compilation != null && CSharpLanguageVersion.IsCSharp11OrAbove(compilation);
    }
}
