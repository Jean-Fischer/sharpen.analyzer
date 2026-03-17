using System;

namespace Sharpen.Analyzer.Sample;

public static class CSharp6Samples
{
    public static void UseNameofExpressionForThrowingArgumentExceptions(string p)
    {
        // SHARPEN012
        throw new ArgumentNullException("p");
    }

    public sealed class ExpressionBodiedGetOnlyPropertySample
    {
        // SHARPEN010
        public int Value => 42;
    }

    public sealed class ExpressionBodiedGetOnlyIndexerSample
    {
        // SHARPEN011
        public int this[int index] => index;
    }

    public sealed class DependencyPropertySample
    {
        // SHARPEN013
        public static readonly DependencyProperty? FooProperty =
            DependencyProperty.Register("Foo", typeof(int), typeof(DependencyPropertySample));

        public int Foo { get; }

        // Minimal stubs so the sample compiles without referencing WPF.
        public sealed class DependencyProperty
        {
            public static DependencyProperty? Register(string name, Type propertyType, Type ownerType)
            {
                return null;
            }

            public static DependencyProperty? RegisterAttached(string name, Type propertyType, Type ownerType)
            {
                return null;
            }
        }
    }
}