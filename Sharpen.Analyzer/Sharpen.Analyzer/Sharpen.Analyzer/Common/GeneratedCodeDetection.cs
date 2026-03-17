using System;
using System.IO;

namespace Sharpen.Analyzer.Common;

public static class GeneratedCodeDetection
{
    // Ported (simplified) from legacy Sharpen. Keep conservative and fast.
    public static bool IsGeneratedFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(fileName))
            return false;

        // Common generated file patterns.
        return fileName.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
               fileName.Equals("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase);
    }
}