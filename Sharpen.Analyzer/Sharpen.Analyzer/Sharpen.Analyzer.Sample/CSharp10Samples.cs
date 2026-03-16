// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Sharpen.Analyzer.Sample;

public static class CSharp10Samples
{
    // SHARPEN040: Use file-scoped namespace
    // NOTE: This is intentionally block-scoped so the analyzer can suggest file-scoped.
    // (This file itself is file-scoped at the top, so keep the example as a string snippet.)

    // SHARPEN042: Use record struct
    public struct Point
    {
        public int X;
        public int Y;
    }

    // SHARPEN043: Use extended property pattern
    public static bool IsAdult(Person p)
    {
        return p is Person x && x.Age > 18;
    }

    // SHARPEN044: Use interpolated string
    public static string Greeting(string name)
    {
        return "Hello, " + name + "!";
    }

    // SHARPEN045: Use const interpolated string
    public const string ConstGreeting = "Hello, " + "World" + "!";

    public sealed class Person
    {
        public int Age { get; set; }
    }
}
