using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Sharpen.Analyzer.Tests;

public sealed class FirstPassSafetyRunnerUsageTests
{
    [Fact]
    public void FirstPassSafetyRunner_IsOnlyUsedByFixProviderSafetyRunner()
    {
        var repoRoot = GetRepositoryRoot();
        var analyzerProjectRoot = Path.Combine(repoRoot, "Sharpen.Analyzer", "Sharpen.Analyzer", "Sharpen.Analyzer");

        var csFiles = Directory.EnumerateFiles(analyzerProjectRoot, "*.cs", SearchOption.AllDirectories)
            // Exclude build outputs (paths like .../obj/Debug/... and .../bin/Debug/...).
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var offenders = new List<string>();

        foreach (var file in csFiles)
        {
            var text = File.ReadAllText(file);

            // Ignore the runner itself.
            if (file.EndsWith(Path.Combine("Safety", "FirstPassSafetyRunner.cs"), StringComparison.OrdinalIgnoreCase))
                continue;

            if (!text.Contains("FirstPassSafetyRunner", StringComparison.Ordinal))
                continue;

            // Allow the unified runner to reference it (if it still does).
            if (file.EndsWith(Path.Combine("Safety", "FixProviderSafety", "FixProviderSafetyRunner.cs"), StringComparison.OrdinalIgnoreCase))
                continue;

            offenders.Add(Relativize(repoRoot, file));
        }

        Assert.True(
            offenders.Count == 0,
            "FirstPassSafetyRunner should not be referenced outside the unified safety pipeline. Offenders:\n" +
            string.Join("\n", offenders));
    }

    private static string GetRepositoryRoot()
    {
        // Tests run from bin/<config>/<tfm>/, so walk up until we find the repo marker.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Readme.md")) && Directory.Exists(Path.Combine(dir.FullName, "openspec")))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root from test base directory: " + AppContext.BaseDirectory);
    }

    private static string Relativize(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative.Replace(Path.DirectorySeparatorChar, '/');
    }
}
