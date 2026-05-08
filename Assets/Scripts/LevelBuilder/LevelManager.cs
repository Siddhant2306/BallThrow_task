using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelBuilder builder;
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private GameManager gameManager;

    [Header("Progress")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private bool startFromUnlockedLevel = false;
    [SerializeField] private string levelsUnlockedKey = "levels_unlocked";

    [Header("Seeds")]
    [SerializeField] private bool deterministicSeeds = true;
    [SerializeField] private int baseSeed = 12345;

    [Header("Behavior")]
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool rebuildLayoutOnRetry = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private int currentLevel;
    private int currentSeed;
    private LevelBuilder.BuiltLevel built;

    private static bool IsSceneObject(Object obj)
    {
        if (obj == null)
            return false;

        if (obj is Component c)
        {
            Scene s = c.gameObject.scene;
            return s.IsValid() && s.isLoaded;
        }

        if (obj is GameObject go)
        {
            Scene s = go.scene;
            return s.IsValid() && s.isLoaded;
        }

        return true;
    }

    private void Awake()
    {
        // Avoid prefab-asset references (prefabs can’t reference scene objects, and calling gameplay methods on prefab assets breaks runtime state).
        if (builder != null && !IsSceneObject(builder))
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] LevelManager: 'builder' reference was a prefab asset; clearing so we can auto-find the scene instance.", this);
            builder = null;
        }

        if (launcher != null && !IsSceneObject(launcher))
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] LevelManager: 'launcher' reference was a prefab asset; clearing so we can auto-find the scene instance.", this);
            launcher = null;
        }

        if (gameManager != null && !IsSceneObject(gameManager))
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] LevelManager: 'gameManager' reference was a prefab asset; clearing so we can auto-find the scene instance.", this);
            gameManager = null;
        }

        if (builder == null)
            builder = GetComponentInChildren<LevelBuilder>(includeInactive: true);

        if (launcher == null)
            launcher = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);

        if (gameManager == null)
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindAnyObjectByType<GameManager>(FindObjectsInactive.Include);

        currentLevel = Mathf.Max(1, startingLevel);
        if (startFromUnlockedLevel)
            currentLevel = Mathf.Max(currentLevel, PlayerPrefs.GetInt(levelsUnlockedKey, currentLevel));

        currentSeed = MakeSeed(currentLevel);
    }

    private void Start()
    {
        if (buildOnStart)
            BuildCurrentLevel(keepSeed: true);
    }

    public int CurrentLevel => currentLevel;
    public int CurrentSeed => currentSeed;

    public void NextLevel()
    {
        currentLevel = Mathf.Max(1, currentLevel + 1);
        currentSeed = MakeSeed(currentLevel);

        if (debugLogs)
            Debug.Log($"[BallThrow] LevelManager.NextLevel -> level={currentLevel} seed={currentSeed}", this);

        BuildCurrentLevel(keepSeed: true);
    }

    public void RetryLevel()
    {
        if (debugLogs)
            Debug.Log($"[BallThrow] LevelManager.RetryLevel level={currentLevel} rebuild={rebuildLayoutOnRetry}", this);

        if (rebuildLayoutOnRetry)
        {
            BuildCurrentLevel(keepSeed: true);
            return;
        }

        if (launcher != null)
            launcher.ResetLauncher();
    }

    public void BuildCurrentLevel(bool keepSeed)
    {
        if (builder == null)
        {
            Debug.LogError("[BallThrow] LevelManager: no LevelBuilder reference set.", this);
            return;
        }

        if (!keepSeed)
            currentSeed = MakeSeed(currentLevel);

        if (built.root != null)
            Destroy(built.root);

        Vector3 spawnBase = launcher != null ? launcher.transform.position : Vector3.zero;
        float zPlane = spawnBase.z;

        built = builder.Build(currentLevel, currentSeed, spawnBase, zPlane, parentOverride: transform);

        if (debugLogs)
            Debug.Log($"[BallThrow] LevelManager: built level={built.level} seed={built.seed} root={(built.root != null ? built.root.name : "null")}", this);

        if (gameManager != null)
            gameManager.SetLevelIndex(currentLevel);

        if (launcher != null && built.spawnPoint != null)
        {
            launcher.SetResetPoint(built.spawnPoint);
            launcher.ResetLauncher();
        }
    }

    private int MakeSeed(int level)
    {
        if (!deterministicSeeds)
            return Random.Range(int.MinValue, int.MaxValue);

        unchecked
        {
            return baseSeed + (level * 10007);
        }
    }
}
