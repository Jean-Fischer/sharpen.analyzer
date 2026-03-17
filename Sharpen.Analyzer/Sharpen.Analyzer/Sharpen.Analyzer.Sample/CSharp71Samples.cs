namespace Sharpen.Analyzer.Sample;

public static class CSharp71Samples
{
    public static int DefaultExpressionInReturnStatement()
    {
        // SHARPEN022
        return default;
    }

    public sealed class DefaultExpressionInOptionalParametersSample
    {
        public DefaultExpressionInOptionalParametersSample(int x = default)
        {
            // SHARPEN024
            _ = x;
        }

        public void M(int x = default)
        {
            // SHARPEN023
            _ = x;
        }
    }
}