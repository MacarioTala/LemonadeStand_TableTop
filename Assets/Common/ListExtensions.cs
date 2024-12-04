
using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static readonly Random rng = new();

    public static List<T> Shuffle<T>(this IList<T> list)
    {
        for(int i=list.Count-1; i>0; i--)
        {
            int j = rng.Next(i+1);
            (list[j], list[i]) = (list[i], list[j]);
        }
        return (List<T>)list;
    }

}