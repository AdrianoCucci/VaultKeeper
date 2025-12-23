using System;

namespace VaultKeeper.Common.Extensions;

public static class ComparableExtensions
{
    public static bool IsBetween<T>(this IComparable<T> number, T min, T max)
    {
        return number.CompareTo(min) >= 0 && number.CompareTo(max) <= 0;
    }
}
