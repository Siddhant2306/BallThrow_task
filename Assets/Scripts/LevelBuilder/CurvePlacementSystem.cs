using UnityEngine;

[System.Serializable]
public class CurvePlacementSettings
{
    [Header("Position (relative to spawn)")]
    public Vector2 xFromSpawnRange = new Vector2(5.5f, 9.5f);
    public Vector2 yFromSpawnRange = new Vector2(-0.5f, 3.0f);

    [Header("Rotation")]
    public Vector2 zRotationRange = new Vector2(-18f, 18f);
}

public static class CurvePlacementSystem
{
    public static void ComputeCurvePose(Vector3 spawnWorld, float zPlane, CurvePlacementSettings settings, System.Random rng, out Vector3 position, out Quaternion rotation)
    {
        if (settings == null)
            settings = new CurvePlacementSettings();

        float x = spawnWorld.x + Rand.Range(rng, settings.xFromSpawnRange.x, settings.xFromSpawnRange.y);
        float y = spawnWorld.y + Rand.Range(rng, settings.yFromSpawnRange.x, settings.yFromSpawnRange.y);

        position = new Vector3(x, y, zPlane);

        float zRot = Rand.Range(rng, settings.zRotationRange.x, settings.zRotationRange.y);
        rotation = Quaternion.Euler(0f, 0f, zRot);
    }
}

