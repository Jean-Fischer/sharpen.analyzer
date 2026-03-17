// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;

namespace Sharpen.Analyzer.Sample;

// If you don't see warnings, build the Analyzers Project.

public class Examples
{
    public void ToStars()
    {
        var spaceship = new Spaceship();
        spaceship.SetSpeed(300000000); // Invalid value, it should be highlighted.
        spaceship.SetSpeed(42);
    }

    public void CSharp8_UsingDeclarationSample()
    {
        // SHARPEN025: Replace using statement with using declaration
        using (var d = new Disposable())
        {
        }
    }

    public class MyCompanyClass // Try to apply quick fix using the IDE.
    {
    }

    private sealed class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}