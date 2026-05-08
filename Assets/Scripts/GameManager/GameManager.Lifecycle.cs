using BallThrow.Gameplay;
using UnityEngine;

public partial class GameManager
{
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Warn("Duplicate GameManager detected; destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (disableVSync)
            QualitySettings.vSyncCount = 0;
        if (targetFrameRate > 0)
            Application.targetFrameRate = targetFrameRate;

        if (autoFindReferences)
        {
            VLog("Resolving references (autoFindReferences=true).");
            ResolveReferences();
        }

        HookLauncher();

        Log("GameManager Awake complete. Entering Idle.");
        SetState(GameState.Idle, force: true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnhookLauncher();
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
