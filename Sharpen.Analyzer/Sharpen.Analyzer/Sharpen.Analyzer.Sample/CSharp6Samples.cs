namespace Sharpen.Analyzer.Sample;

public static class CSharp6Samples
{
    public sealed class ExpressionBodiedGetOnlyPropertySample
    {
        // SHARPEN010
        public int Value
        {
            get
            {
                return 42;
            }
        }
    }

    public sealed class ExpressionBodiedGetOnlyIndexerSample
    {
        // SHARPEN011
        public int this[int index]
        {
            get
            {
                return index;
            }
        }
    }

    public static void UseNameofExpressionForThrowingArgumentExceptions(string p)
    {
        // SHARPEN012
        throw new System.ArgumentNullException("p");
    }

    public sealed class DependencyPropertySample
    {
        // Minimal stubs so the sample compiles without referencing WPF.
        public sealed class DependencyProperty
        {
            public static DependencyProperty? Register(string name, System.Type propertyType, System.Type ownerType) => null;

            public static DependencyProperty? RegisterAttached(string name, System.Type propertyType, System.Type ownerType) => null;
        }

        // SHARPEN013
        public static readonly DependencyProperty? FooProperty =
            DependencyProperty.Register("Foo", typeof(int), typeof(DependencyPropertySample));

        public int Foo { get; }
    }
}
