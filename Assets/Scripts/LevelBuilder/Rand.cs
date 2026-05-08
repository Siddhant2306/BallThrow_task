using UnityEngine;

public static class Rand
{
    public static float Range(System.Random rng, float minInclusive, float maxInclusive)
    {
        if (rng == null)
            return Random.Range(minInclusive, maxInclusive);

        if (minInclusive > maxInclusive)
        {
            float tmp = minInclusive;
            minInclusive = maxInclusive;
            maxInclusive = tmp;
        }

        double t = rng.NextDouble(); // [0,1)
        return (float)(minInclusive + (maxInclusive - minInclusive) * t);
    }

    public static int Range(System.Random rng, int minInclusive, int maxExclusive)
    {
        if (rng == null)
            return Random.Range(minInclusive, maxExclusive);

        if (minInclusive >= maxExclusive)
            return minInclusive;

        return rng.Next(minInclusive, maxExclusive);
    }

    public static bool Chance(System.Random rng, float probability01)
    {
        probability01 = Mathf.Clamp01(probability01);
        return Range(rng, 0f, 1f) <= probability01;
    }
}

