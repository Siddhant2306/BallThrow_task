using BallThrow.Gameplay;
using UnityEngine;

public partial class GameManager
{
    public bool TrySuccess(Rigidbody triggeredBallBody, Transform goalTransform, float minSpeed)
    {
        if (State == GameState.Failed || State == GameState.Success)
            return false;

        if (ballBody == null && launcher != null)
            ballBody = launcher.GetComponent<Rigidbody>();

        if (ballBody == null)
            return false;

        if (triggeredBallBody == null || triggeredBallBody != ballBody)
            return false;

        if (triggeredBallBody.linearVelocity.magnitude < minSpeed)
            return false;

        Log($"TrySuccess: accepted speed={triggeredBallBody.linearVelocity.magnitude:0.00} min={minSpeed:0.00}");
        Succeed(goalTransform);
        return true;
    }

    private void Succeed(Transform goalTransform)
    {
        if (State is GameState.Success or GameState.Failed)
            return;

        Log("Success triggered.");
        SetState(GameState.Success);
        StopInput();

        if (ballState != null)
            ballState.MarkSuccess();

        if (unlockNextLevelOnSuccess)
            UnlockNextLevel();

        if (successHandler != null)
            successHandler.PlaySuccess(ballBody);

        if (ui != null)
        {
            ui.HideEndScreen();
            ui.ShowSuccess(onNext: NextLevel, onRetry: RetryLevel);
        }
    }

    private void UnlockNextLevel()
    {
        int unlocked = PlayerPrefs.GetInt(levelsUnlockedKey, 1);
        int next = Mathf.Max(unlocked, levelIndex + 1);
        PlayerPrefs.SetInt(levelsUnlockedKey, next);
        PlayerPrefs.Save();
    }
}
