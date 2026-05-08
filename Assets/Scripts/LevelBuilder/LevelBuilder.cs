using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelBuilder : MonoBehaviour
{
    [Serializable]
    public struct BuiltLevel
    {
        public int level;
        public int seed;
        public GameObject root;
        public Transform spawnPoint;
        public GameObject curve;
        public GameObject goal;
        public GameObject failZone;
        public List<GameObject> staticObstacles;
        public List<GameObject> movingObstacles;
        public List<GameObject> decor;
    }

    [Header("Difficulty")]
    [SerializeField] private LevelDifficultyTuning difficulty = new LevelDifficultyTuning();

    [Header("Prefabs")]
    [SerializeField] private GameObject spawnPointPrefab;
    [SerializeField] private GameObject curvePrefab;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private GameObject failZonePrefab;
    [SerializeField] private GameObject[] staticObstaclePrefabs;
    [SerializeField] private GameObject[] movingObstaclePrefabs;
    [SerializeField] private GameObject[] decorPrefabs;

    [Header("Placement")]
    [SerializeField] private Vector2 spawnJitter = new Vector2(0.4f, 0.2f);
    [SerializeField] private CurvePlacementSettings curvePlacement = new CurvePlacementSettings();
    [SerializeField] private GoalSpawnSettings goalSpawn = new GoalSpawnSettings();
    [SerializeField] private ObstacleSpawnSettings obstacleSpawn = new ObstacleSpawnSettings();

    [Header("Fail Zone Placement (relative to spawn)")]
    [SerializeField] private Vector2 failZoneOffset = new Vector2(9f, -7f);

    [Header("Decor")]
    [SerializeField] private int decorMin = 0;
    [SerializeField] private int decorMax = 3;
    [SerializeField] private Vector2 decorXRange = new Vector2(-4f, 22f);
    [SerializeField] private Vector2 decorYRange = new Vector2(-3f, 12f);

    [Header("Camera Safe View")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float viewportPadding = 0.8f;
    [SerializeField] private bool clampGeneratedObjectsToCamera = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private struct CameraWorldBounds
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;
    }

    private CameraWorldBounds GetCameraWorldBounds(float zPlane)
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;

        float distance = Mathf.Abs(cam.transform.position.z - zPlane);

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, distance));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, distance));

        float horizontalPadding = 4f;
        float verticalPadding = 2.4f;

        return new CameraWorldBounds
        {
            minX = bottomLeft.x + horizontalPadding,
            maxX = topRight.x - horizontalPadding,

            minY = bottomLeft.y + verticalPadding,
            maxY = topRight.y - verticalPadding
        };
    }

    private Vector3 ClampToCameraBounds(Vector3 pos, CameraWorldBounds bounds)
    {
        pos.x = Mathf.Clamp(pos.x, bounds.minX, bounds.maxX);
        pos.y = Mathf.Clamp(pos.y, bounds.minY, bounds.maxY);
        return pos;
    }



    public BuiltLevel Build(int level, int seed, Vector3 spawnBaseWorld, float zPlane, Transform parentOverride = null)
    {
        var rng = new System.Random(seed);
        LevelDifficultySettings d = LevelDifficulty.Evaluate(level, difficulty);
        CameraWorldBounds cameraBounds = GetCameraWorldBounds(zPlane);

        if (debugLogs)
            Debug.Log($"[BallThrow] LevelBuilder.Build level={level} seed={seed} t={d.t01:0.00}", this);

        Transform parent = parentOverride != null ? parentOverride : transform;

        GameObject root = new GameObject($"Level_{level}_Seed_{seed}");
        root.transform.SetParent(parent, worldPositionStays: false);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        Vector3 spawnPos = new Vector3(
        spawnBaseWorld.x + Rand.Range(rng, -spawnJitter.x, spawnJitter.x),
        spawnBaseWorld.y + Rand.Range(rng, -spawnJitter.y, spawnJitter.y),
        zPlane);

        if (clampGeneratedObjectsToCamera){
            spawnPos = ClampToCameraBounds(spawnPos, cameraBounds);
        }
        Transform spawnPoint = SpawnSpawnPoint(root.transform, spawnPos);

        CurvePlacementSystem.ComputeCurvePose(spawnPos, zPlane, curvePlacement, rng, out Vector3 curvePos, out Quaternion curveRot);
        if (clampGeneratedObjectsToCamera)
        {
            curvePos = ClampToCameraBounds(curvePos, cameraBounds);
        }
        GameObject curve = SpawnOptional(curvePrefab, root.transform, curvePos, curveRot, "curve");

        GameObject goal = GoalSpawner.SpawnGoal(goalPrefab, root.transform, curvePos, zPlane, goalSpawn, rng, d.goalScale, debugLogs);

        if (clampGeneratedObjectsToCamera && goal != null)
        {
            goal.transform.position = ClampToCameraBounds(goal.transform.position, cameraBounds);
        }

        Vector3 failPos = new Vector3(spawnPos.x + failZoneOffset.x, spawnPos.y + failZoneOffset.y, zPlane);

        if (clampGeneratedObjectsToCamera)
        {
            failPos = ClampToCameraBounds(failPos, cameraBounds);
        }

        GameObject failZone = SpawnOptional(failZonePrefab, root.transform, failPos, Quaternion.identity, "failZone");

        Vector3 curveWorldForAvoid = curve != null ? curve.transform.position : curvePos;
        Vector3 goalWorldForAvoid = goal != null ? goal.transform.position : (curvePos + Vector3.right * 8f);

        List<GameObject> staticObstacles = ObstacleSpawner.SpawnStaticObstacles(
            staticObstaclePrefabs,
            root.transform,
            d.staticObstacleCount,
            spawnPos,
            curveWorldForAvoid,
            goalWorldForAvoid,
            zPlane,
            d.safeCorridorWidth,
            d.obstacleSpacing,
            obstacleSpawn,
            rng,
            debugLogs);

            if (clampGeneratedObjectsToCamera)
            {
                foreach (GameObject obstacle in staticObstacles)
                {
                    if (obstacle != null)
                   obstacle.transform.position = ClampToCameraBounds(obstacle.transform.position, cameraBounds);
                }
            }

        List<GameObject> movingObstacles = ObstacleSpawner.SpawnMovingObstacles(
            movingObstaclePrefabs,
            root.transform,
            d.movingObstacleCount,
            level,
            spawnPos,
            curveWorldForAvoid,
            goalWorldForAvoid,
            zPlane,
            d.safeCorridorWidth,
            d.obstacleSpacing,
            d.movingAmplitude,
            d.movingSpeed,
            obstacleSpawn,
            rng,
            debugLogs);

            if (clampGeneratedObjectsToCamera)
            {
                foreach (GameObject obstacle in movingObstacles)
                {
                    if (obstacle != null)
                    obstacle.transform.position = ClampToCameraBounds(obstacle.transform.position, cameraBounds);
                }
            }

        List<GameObject> decor = SpawnDecor(root.transform, rng, d.t01, zPlane);

        return new BuiltLevel
        {
            level = level,
            seed = seed,
            root = root,
            spawnPoint = spawnPoint,
            curve = curve,
            goal = goal,
            failZone = failZone,
            staticObstacles = staticObstacles,
            movingObstacles = movingObstacles,
            decor = decor,
        };
    }

    private Transform SpawnSpawnPoint(Transform parent, Vector3 worldPosition)
    {
        if (spawnPointPrefab != null)
        {
            GameObject go = Instantiate(spawnPointPrefab, worldPosition, Quaternion.identity, parent);
            if (debugLogs)
                Debug.Log($"[BallThrow] LevelBuilder: spawned spawnPoint '{go.name}' at {worldPosition}", go);
            return go.transform;
        }

        GameObject empty = new GameObject("SpawnPoint");
        empty.transform.SetParent(parent, worldPositionStays: false);
        empty.transform.position = worldPosition;
        empty.transform.rotation = Quaternion.identity;
        empty.transform.localScale = Vector3.one;

        if (debugLogs)
            Debug.Log($"[BallThrow] LevelBuilder: created spawnPoint at {worldPosition}", empty);

        return empty.transform;
    }

    private GameObject SpawnOptional(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot, string label)
    {
        if (prefab == null)
        {
            if (debugLogs)
                Debug.LogWarning($"[BallThrow] LevelBuilder: {label} prefab not assigned.");
            return null;
        }

        GameObject go = Instantiate(prefab, pos, rot, parent);
        if (debugLogs)
            Debug.Log($"[BallThrow] LevelBuilder: spawned {label} '{go.name}' at {pos}", go);
        return go;
    }

    private List<GameObject> SpawnDecor(Transform parent, System.Random rng, float difficulty01, float zPlane)
    {
        var spawned = new List<GameObject>(4);
        if (decorPrefabs == null || decorPrefabs.Length == 0)
            return spawned;

        int count = Mathf.RoundToInt(Mathf.Lerp(decorMin, decorMax, Mathf.Clamp01(difficulty01)));
        for (int i = 0; i < count; i++)
        {
            GameObject prefab = decorPrefabs[Rand.Range(rng, 0, decorPrefabs.Length)];
            if (prefab == null)
                continue;

            CameraWorldBounds bounds = GetCameraWorldBounds(zPlane);

            Vector3 pos = new Vector3(
                Rand.Range(rng, bounds.minX, bounds.maxX),
                Rand.Range(rng, bounds.minY, bounds.maxY),
                zPlane);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);
            spawned.Add(go);

            if (debugLogs)
                Debug.Log($"[BallThrow] LevelBuilder: spawned decor '{go.name}' at {pos}", go);
        }

        return spawned;
    }
}
