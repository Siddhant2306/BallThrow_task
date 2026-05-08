using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObstacleSpawnSettings
{
    [Header("Spawn Zone (world)")]
    public Vector2 xRange = new Vector2(2f, 18f);
    public Vector2 yRange = new Vector2(-1f, 10f);

    [Header("Avoidance")]
    public float minDistanceFromSpawn = 2.5f;
    public float minDistanceFromCurve = 2.0f;
    public float minDistanceFromGoal = 2.0f;

    [Header("Placement")]
    public int maxPlacementAttemptsPerObstacle = 60;
    public Vector2 zRotationRange = new Vector2(0f, 360f);
}

public static class ObstacleSpawner
{
    public static List<GameObject> SpawnStaticObstacles(
        GameObject[] prefabs,
        Transform parent,
        int count,
        Vector3 spawnWorld,
        Vector3 curveWorld,
        Vector3 goalWorld,
        float zPlane,
        float safeCorridorWidth,
        float obstacleSpacing,
        ObstacleSpawnSettings settings,
        System.Random rng,
        bool debugLogs)
    {
        var spawned = new List<GameObject>(count);
        if (prefabs == null || prefabs.Length == 0 || count <= 0)
            return spawned;

        if (settings == null)
            settings = new ObstacleSpawnSettings();

        var used = new List<Vector2>(count + 8);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Rand.Range(rng, 0, prefabs.Length)];
            if (prefab == null)
                continue;

            bool placed = TryFindObstaclePosition(
                spawnWorld,
                curveWorld,
                goalWorld,
                zPlane,
                safeCorridorWidth,
                obstacleSpacing,
                settings,
                rng,
                used,
                out Vector3 pos);

            if (!placed)
            {
                if (debugLogs)
                    Debug.LogWarning($"[BallThrow] ObstacleSpawner: could not place static obstacle {i + 1}/{count} (ran out of attempts).");
                continue;
            }

            float zRot = Rand.Range(rng, settings.zRotationRange.x, settings.zRotationRange.y);
            Quaternion rot = Quaternion.Euler(0f, 0f, zRot);

            GameObject go = Object.Instantiate(prefab, pos, rot, parent);
            spawned.Add(go);
            used.Add(new Vector2(pos.x, pos.y));

            if (debugLogs)
                Debug.Log($"[BallThrow] ObstacleSpawner: spawned static '{go.name}' at {pos}", go);
        }

        return spawned;
    }

    public static List<GameObject> SpawnMovingObstacles(
        GameObject[] prefabs,
        Transform parent,
        int count,
        int level,
        Vector3 spawnWorld,
        Vector3 curveWorld,
        Vector3 goalWorld,
        float zPlane,
        float safeCorridorWidth,
        float obstacleSpacing,
        float movingAmplitude,
        float movingSpeed,
        ObstacleSpawnSettings settings,
        System.Random rng,
        bool debugLogs)
    {
        var spawned = new List<GameObject>(count);
        if (prefabs == null || prefabs.Length == 0 || count <= 0)
            return spawned;

        if (settings == null)
            settings = new ObstacleSpawnSettings();

        var used = new List<Vector2>(count + 8);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Rand.Range(rng, 0, prefabs.Length)];
            if (prefab == null)
                continue;

            bool placed = TryFindObstaclePosition(
                spawnWorld,
                curveWorld,
                goalWorld,
                zPlane,
                safeCorridorWidth,
                obstacleSpacing,
                settings,
                rng,
                used,
                out Vector3 pos);

            if (!placed)
            {
                if (debugLogs)
                    Debug.LogWarning($"[BallThrow] ObstacleSpawner: could not place moving obstacle {i + 1}/{count} (ran out of attempts).");
                continue;
            }

            GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
            spawned.Add(go);
            used.Add(new Vector2(pos.x, pos.y));

            MovingObstacle mover = go.GetComponent<MovingObstacle>();
            if (mover == null)
            {
                mover = go.AddComponent<MovingObstacle>();
                if (debugLogs)
                    Debug.LogWarning($"[BallThrow] ObstacleSpawner: moving prefab '{prefab.name}' had no MovingObstacle; added one at runtime.", go);
            }

            mover.Configure(ChooseType(level, rng), movingAmplitude, movingSpeed);

            if (debugLogs)
                Debug.Log($"[BallThrow] ObstacleSpawner: spawned moving '{go.name}' at {pos} type={mover.Type}", go);
        }

        return spawned;
    }

    private static bool TryFindObstaclePosition(
        Vector3 spawnWorld,
        Vector3 curveWorld,
        Vector3 goalWorld,
        float zPlane,
        float safeCorridorWidth,
        float obstacleSpacing,
        ObstacleSpawnSettings settings,
        System.Random rng,
        List<Vector2> used,
        out Vector3 position)
    {
        Vector2 spawn2 = new Vector2(spawnWorld.x, spawnWorld.y);
        Vector2 goal2 = new Vector2(goalWorld.x, goalWorld.y);
        Vector2 curve2 = new Vector2(curveWorld.x, curveWorld.y);

        int attempts = Mathf.Max(1, settings.maxPlacementAttemptsPerObstacle);
        for (int a = 0; a < attempts; a++)
        {
            float x = Rand.Range(rng, settings.xRange.x, settings.xRange.y);
            float y = Rand.Range(rng, settings.yRange.x, settings.yRange.y);
            Vector2 p = new Vector2(x, y);

            if (Vector2.Distance(p, spawn2) < settings.minDistanceFromSpawn)
                continue;
            if (Vector2.Distance(p, curve2) < settings.minDistanceFromCurve)
                continue;
            if (Vector2.Distance(p, goal2) < settings.minDistanceFromGoal)
                continue;

            if (safeCorridorWidth > 0f && DistancePointToSegment(p, spawn2, goal2) < safeCorridorWidth)
                continue;

            bool tooClose = false;
            float minDist = Mathf.Max(0.01f, obstacleSpacing);
            for (int i = 0; i < used.Count; i++)
            {
                if (Vector2.Distance(p, used[i]) < minDist)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
                continue;

            position = new Vector3(x, y, zPlane);
            return true;
        }

        position = default;
        return false;
    }

    private static float DistancePointToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float abSqr = ab.sqrMagnitude;
        if (abSqr <= 0.000001f)
            return Vector2.Distance(p, a);

        float t = Vector2.Dot(p - a, ab) / abSqr;
        t = Mathf.Clamp01(t);
        Vector2 proj = a + ab * t;
        return Vector2.Distance(p, proj);
    }

    private static MovingObstacle.MovementType ChooseType(int level, System.Random rng)
    {
        // Controlled variety. Early levels = readable movers; later = more punishing.
        int roll = Rand.Range(rng, 0, 100);

        if (level >= 8 && roll < 20)
            return MovingObstacle.MovementType.Oscillate;

        if (level >= 6 && roll < 45)
            return MovingObstacle.MovementType.Pendulum;

        if (roll < 65)
            return MovingObstacle.MovementType.Horizontal;

        if (roll < 85)
            return MovingObstacle.MovementType.Vertical;

        return MovingObstacle.MovementType.Rotator;
    }
}

