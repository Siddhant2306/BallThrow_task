using UnityEngine.SceneManagement;

public partial class GameManager
{
    public void RetryLevel()
    {
        Log("RetryLevel requested.");
        RestoreTimeScale();

        if (launcher != null)
        {
            launcher.ResetLauncher();
            return;
        }

        Warn("Retry requested, but GameManager has no ProjectileLauncher reference. Assign it in the Inspector.");
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
