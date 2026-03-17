namespace Sharpen.Analyzer.Sample;

public sealed class UseExpressionBodyForSetAccessorsInPropertiesSample
{
    public int P
    {
        get;

        // SHARPEN056
        set;
    }

    public int AlreadyExpressionBodied
    {
        get => P;
        set => P = value;
    }
}