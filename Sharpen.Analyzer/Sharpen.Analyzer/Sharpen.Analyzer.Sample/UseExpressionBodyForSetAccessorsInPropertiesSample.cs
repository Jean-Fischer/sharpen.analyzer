namespace Sharpen.Analyzer.Sample;

public sealed class UseExpressionBodyForSetAccessorsInPropertiesSample
{
    private int _p;

    public int P
    {
        get => _p;

        // SHARPEN056
        set { _p = value; }
    }

    public int AlreadyExpressionBodied
    {
        get => _p;
        set => _p = value;
    }
}
