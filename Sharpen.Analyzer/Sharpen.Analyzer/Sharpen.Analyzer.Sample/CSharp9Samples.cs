// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;

namespace Sharpen.Analyzer.Sample;

public static class CSharp9Samples
{
    // SHARPEN035: Use init-only setter
    public sealed class InitOnlySetterSample
    {
        public InitOnlySetterSample(string name)
        {
            Name = name;
        }

        public string Name { get; private set; } = string.Empty;
    }

    // SHARPEN036: Use record type
    public sealed class RecordTypeSample
    {
        public RecordTypeSample(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    // SHARPEN037: Use top-level statements
    // NOTE: This sample is intentionally a classic Program/Main entry point.
    // The analyzer/code fix should suggest converting it to top-level statements.
    public static class TopLevelStatementsSample
    {
        public static int Main(string[] args)
        {
            Console.WriteLine(args.Length);
            return 0;
        }
    }

    // SHARPEN038: Use C# 9 pattern matching
    public static class PatternMatchingSample
    {
        public static bool IsNotNull(object o)
        {
            return o != null;
        }

        public static bool IsNotString(object o)
        {
            return !(o is string);
        }

        public static bool IsInRange(int x)
        {
            return x >= 0 && x <= 10;
        }

        public static bool IsOutsideRange(int x)
        {
            // Use constants here so the relational-pattern rewrite is valid.
            return x < 0 || x > 10;
        }
    }

    // SHARPEN039: Use target-typed new
    public sealed class TargetTypedNewSample
    {
        private readonly List<int> _numbers = new();

        public List<int> Numbers { get; } = new();

        public Dictionary<string, List<int>> Map { get; } = new();

        public List<int> Create()
        {
            return new List<int>();
        }
    }
}