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

        // Preserve the prefab's authored rotation/scale (then apply difficulty scaling).
        Quaternion authoredRotation = prefab.transform.rotation;

        // Instantiate without parenting first, then parent with worldPositionStays=true to preserve scale even if the parent has non-1 scale.
        GameObject goal = Object.Instantiate(prefab, pos, authoredRotation);
        if (parent != null)
            goal.transform.SetParent(parent, worldPositionStays: true);

        float scaleMul = Mathf.Max(0.05f, goalScale01);
        if (!Mathf.Approximately(scaleMul, 1f))
            goal.transform.localScale *= scaleMul;

        // Ensure TargetNextLevel receives trigger/collision messages even if the collider is on a child object.
        TargetNextLevel target = goal.GetComponentInChildren<TargetNextLevel>(includeInactive: true);
        if (target != null)
        {
            Rigidbody existing = target.GetComponent<Rigidbody>();
            if (existing == null)
            {
                // If there is already a Rigidbody somewhere else in this prefab hierarchy, adding another can break compound collider behaviour.
                // In that case we don't auto-add; instead we log a clear warning.
                Rigidbody anyRb = goal.GetComponentInChildren<Rigidbody>(includeInactive: true);
                if (anyRb != null)
                {
                    if (debugLogs)
                        Debug.LogWarning($"[BallThrow] GoalSpawner: goal has TargetNextLevel on '{target.name}' but Rigidbody exists on '{anyRb.name}'. Move TargetNextLevel onto the Rigidbody object (or remove the extra Rigidbody).", goal);
                }
                else
                {
                    Rigidbody rb = target.gameObject.AddComponent<Rigidbody>();
                    rb.isKinematic = true;
                    rb.useGravity = false;

                    if (debugLogs)
                        Debug.Log($"[BallThrow] GoalSpawner: added kinematic Rigidbody to '{target.name}' so TargetNextLevel receives collisions.", target);
                }
            }
        }
        else if (debugLogs)
        {
            Debug.LogWarning($"[BallThrow] GoalSpawner: spawned goal '{prefab.name}' but found no TargetNextLevel in its hierarchy.", goal);
        }

        if (debugLogs)
            Debug.Log($"[BallThrow] GoalSpawner: spawned goal '{goal.name}' at {pos} rot={authoredRotation.eulerAngles} scaleMul={goalScale01:0.00}", goal);

        return goal;
    }
}
