using UnityEngine;

[System.Serializable]
public class GoalSpawnSettings
{
    [Header("Position (relative to curve)")]
    public Vector2 xFromCurveRange = new Vector2(5.5f, 10.5f);
    public Vector2 yFromCurveRange = new Vector2(0.5f, 5.5f);
}

public static class GoalSpawner
{
    public static GameObject SpawnGoal(GameObject prefab, Transform parent, Vector3 curveWorld, float zPlane, GoalSpawnSettings settings, System.Random rng, float goalScale01, bool debugLogs)
    {
        if (prefab == null)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] GoalSpawner: goal prefab not assigned.");
            return null;
        }

        if (settings == null)
            settings = new GoalSpawnSettings();

        Vector3 pos = new Vector3(
            curveWorld.x + Rand.Range(rng, settings.xFromCurveRange.x, settings.xFromCurveRange.y),
            curveWorld.y + Rand.Range(rng, settings.yFromCurveRange.x, settings.yFromCurveRange.y),
            zPlane);

        GameObject goal = Object.Instantiate(prefab, pos, Quaternion.identity, parent);
        goal.transform.localScale *= Mathf.Max(0.05f, goalScale01);

        if (debugLogs)
            Debug.Log($"[BallThrow] GoalSpawner: spawned goal '{goal.name}' at {pos} scaleMul={goalScale01:0.00}", goal);

        return goal;
    }
}

