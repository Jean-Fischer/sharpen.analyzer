// BEFORE
//
// public int this[int i]
// {
//     get { return _i; }
//     set { }
// }
//
// AFTER
//
// public int this[int i]
// {
//     get => _i;
//     set { }
// }

namespace Sharpen.Analyzer.Samples.CSharp7.ExpressionBodiedMembers;

public class UseExpressionBodyForGetAccessorsInIndexersSample
{
    private int _i;

    public int this[int i]
    {
        get { return _i; }
        set { }
    }
}
