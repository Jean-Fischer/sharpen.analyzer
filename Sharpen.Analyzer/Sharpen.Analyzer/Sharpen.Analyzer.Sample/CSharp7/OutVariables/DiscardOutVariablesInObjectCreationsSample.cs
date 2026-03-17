namespace Sharpen.Analyzer.Samples.CSharp7.OutVariables;

public static class DiscardOutVariablesInObjectCreationsSample
{
    public static void Run()
    {
        // Before (candidate):
        // int value;
        // _ = new C(out value);
        //
        // After:
        // _ = new C(out _);

        int value;
        _ = new C(out value);
    }

    private sealed class C
    {
        public C(out int value)
        {
            value = 123;
        }
    }
}