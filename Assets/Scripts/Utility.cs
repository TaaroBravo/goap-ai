using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public static class Utility
{
    public static T Log<T>(T value, string prefix = "")
    {
        Debug.Log(prefix + value);
        return value;
    }

    public static IEnumerable<T> Generate<T>(T seed, Func<T, T> mutate)
    {
        var accum = seed;
        while (true)
        {
            yield return accum;
            accum = mutate(accum);
        }
    }

    public static bool In<T>(this T x, HashSet<T> set)
    {
        return set.Contains(x);
    }

    public static bool In<K, V>(this KeyValuePair<K, V> x, Dictionary<K, V> dict)
    {
        return dict.Contains(x);
    }

    public static void UpdateWith<K, V>(this Dictionary<K, V> a, Dictionary<K, V> b)
    {
        foreach (var kvp in b)
        {
            a[kvp.Key] = kvp.Value;
        }
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
    {
        return new HashSet<T>(list);
    }

    public static V DefaultGet<K, V>(this Dictionary<K, V> dict,K key,Func<V> defaultFactory)
    {
        V v;
        if (!dict.TryGetValue(key, out v))
            dict[key] = v = defaultFactory();
        return v;
    }

    public static int Clampi(int v, int min, int max)
    {
        return v < min ? min : (v > max ? max : v);
    }

    //public static T Log<T>(T param, string message = "")
    //{
    //    Debug.Log(message + param.ToString());
    //    return param;
    //}

    //public static IEnumerable<Src> Generate<Src>(Src seed, Func<Src, Src> generator)
    //{
    //    while (true)
    //    {
    //        yield return seed;
    //        seed = generator(seed);
    //    }
    //}

    public static IEnumerable<T> AStar<T>(T start,
                                         Func<T, bool> targetCheck,
                                         Func<T, IEnumerable<Tuple<T, float>>> GetNeighbours,
                                         Func<T, float> GetHeuristic)
    {
        HashSet<T> visited = new HashSet<T>();
        Dictionary<T, T> previous = new Dictionary<T, T>();
        Dictionary<T, float> actualDistances = new Dictionary<T, float>();
        Dictionary<T, float> heuristicDistances = new Dictionary<T, float>();
        List<T> pending = new List<T>();
        pending.Add(start);
        actualDistances.Add(start, 0f);
        heuristicDistances.Add(start, GetHeuristic(start));

        while (pending.Any())
        {
            var current = pending.OrderBy(x => heuristicDistances[x]).First();
            pending.Remove(current);
            visited.Add(current);

            if (targetCheck(current))
            {
                return Utility.Generate(current, x => previous[x])
                                .TakeWhile(x => previous.ContainsKey(x))
                                .Reverse();
            }
            else
            {
                var n = GetNeighbours(current).Where(x => !visited.Contains(x.Item1));
                foreach (var elem in n)
                {
                    if(elem.Item1 == null)
                        Debug.Log("AAA");
                    var altDist = actualDistances[current] + elem.Item2 + GetHeuristic(elem.Item1);
                    var currentDist = heuristicDistances.ContainsKey(elem.Item1) ? heuristicDistances[elem.Item1] : float.MaxValue;

                    if (currentDist > altDist)
                    {
                        heuristicDistances[elem.Item1] = altDist;
                        actualDistances[elem.Item1] = actualDistances[current] + elem.Item2;
                        previous[elem.Item1] = current;
                        pending.Add(elem.Item1);
                    }
                }
            }
        }
        return Enumerable.Empty<T>();
    }
}
