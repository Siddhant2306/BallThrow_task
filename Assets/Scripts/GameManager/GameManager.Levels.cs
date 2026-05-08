using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager
{
    public void SetLevelIndex(int index)
    {
        levelIndex = Mathf.Max(1, index);
        VLog($"SetLevelIndex -> {levelIndex}");
    }

    public void RetryLevel()
    {
        Log("RetryLevel requested.");
        RestoreTimeScale();

        if (launcher == null)
        {
            // Scene-based levels: retry means "reset the current ball attempt", not rebuild/destroy the scene.
            ResolveReferences();
            HookLauncher();
        }

        if (launcher != null)
        {
            launcher.ResetLauncher();
            return;
        }

        Warn("Retry requested, but no ProjectileLauncher was found to reset.");
    }

    public void NextLevel()
    {
        Log("NextLevel requested.");
        RestoreTimeScale();

        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentBuildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Warn("Next level requested, but there is no next scene in Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextIndex);
    }
}
