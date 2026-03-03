using System.Collections.Generic;

namespace Sharpen.Analyzer.Sample;

public class Example
{
    public void TriggerVarSuggestion()
    {
        // This will trigger the suggestion:
        List<string> names = new List<string>(); // Warning: Use 'var' instead of explicit type 'List<string>'

        // This will NOT trigger the suggestion (already uses var):
        var ages = new List<int>();

        // This will NOT trigger the suggestion (not an object creation):
        string greeting = "Hello"; 
        _ = greeting;
        // This will NOT trigger the suggestion (type mismatch):
        object data = new List<string>();
    }
}