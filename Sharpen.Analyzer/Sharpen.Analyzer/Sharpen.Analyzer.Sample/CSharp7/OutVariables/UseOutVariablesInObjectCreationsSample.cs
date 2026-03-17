namespace Sharpen.Analyzer.Samples.CSharp7.OutVariables;

public static class UseOutVariablesInObjectCreationsSample
{
    public static void M()
    {
        int x;
        _ = new C(out x);

        // Suggestion: inline the out variable
        // _ = new C(out var x);
    }

    private sealed class C
    {
        public C(out int value)
        {
            value = 42;
        }
    }
}