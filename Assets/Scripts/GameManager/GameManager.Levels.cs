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

        LevelManager levelManager = FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
        if (levelManager != null)
        {
            levelManager.RetryLevel();
            return;
        }

        if (launcher != null)
            launcher.ResetLauncher();
        else
            Warn("Retry requested, but no launcher found.");
    }

    public void NextLevel()
    {
        Log("NextLevel requested.");
        RestoreTimeScale();

        LevelManager levelManager = FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
        if (levelManager != null)
        {
            levelManager.NextLevel();
            return;
        }

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
