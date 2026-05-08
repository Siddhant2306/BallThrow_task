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

    private void HookLauncher()
    {
        if (launcher == null)
        {
            launcher = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);
            if (launcher == null)
            {
                Warn("ProjectileLauncher reference missing on GameManager (and none found in scene).");
                return;
            }

            VLog($"HookLauncher: auto-bound launcher '{launcher.name}'.");
        }

        if (ballBody == null)
            ballBody = launcher.GetComponent<Rigidbody>();

        if (ballState == null)
            ballState = launcher.GetComponent<BallStateController>();

        VLog("HookLauncher: subscribing launcher events.");

        launcher.Launched -= HandleLaunched;
        launcher.Launched += HandleLaunched;

        launcher.Reset -= HandleReset;
        launcher.Reset += HandleReset;
    }

    private void UnhookLauncher()
    {
        if (launcher == null)
            return;

        VLog("UnhookLauncher: unsubscribing launcher events.");

        launcher.Launched -= HandleLaunched;
        launcher.Reset -= HandleReset;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
