using System;

public static class Helpers
{
    // will hash any amount of any object
    public static int MultiHash(params object[] values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        int hash = 17;
        foreach (var value in values)
        {
            hash = hash * 23 + (value?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
