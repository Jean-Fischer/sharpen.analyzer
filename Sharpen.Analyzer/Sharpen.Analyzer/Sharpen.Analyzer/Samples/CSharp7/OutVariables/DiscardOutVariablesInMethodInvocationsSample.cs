namespace Sharpen.Analyzer.Samples.CSharp7.OutVariables;

public static class DiscardOutVariablesInMethodInvocationsSample
{
    public static void Run()
    {
        // Before (candidate):
        // int value;
        // int.TryParse("123", out value);
        //
        // After:
        // int.TryParse("123", out _);

        int value;
        _ = int.TryParse("123", out value);
    }
}
