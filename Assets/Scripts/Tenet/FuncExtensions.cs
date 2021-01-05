using System;

public static class FuncExtensions
{
    public static TOut _<TIn, TOut>(this TIn x, Func<TIn, TOut> f)
    {
        return f(x);
    }

    public static void _<TIn>(this TIn x, Action<TIn> p)
    {
        p(x);
    }
}
