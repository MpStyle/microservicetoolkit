using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace microservice.toolkit.core.extension;

public static class ArrayExtension
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[] l)
    {
        return l == null || l.Length != 0 == false;
    }

    public static T[] ConcatArrays<T>(this T[] array, params T[][] p)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (p == null)
        {
            return array;
        }

        return p.Aggregate(array, (current, next) => current.Concat(next).ToArray());
    }
}