using System;

public static class CastExtensions
{
    public static T As<T>(this object origin) where T : class
    {
        var casted = origin as T;
        if (casted == null)
            throw new Exception($"Can't cast type '{origin.GetType()}' to '{typeof(T)}'");

        return casted;
    }
}
