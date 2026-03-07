namespace Sharpen.Analyzer.Samples.CSharp7.OutVariables;

public static class DiscardOutVariablesInObjectCreationsSample
{
    private sealed class C
    {
        public C(out int value)
        {
            value = 123;
        }
    }

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
}
