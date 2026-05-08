using BallThrow.Gameplay;
using UnityEngine;

public partial class GameManager
{
    public bool Fail(FailureReason reason, Vector3 at, Collider hitCollider = null)
    {
        if (State is GameState.Success or GameState.Failed)
            return false;

        Log($"Fail: reason={reason} at={at} hit={(hitCollider != null ? hitCollider.name : "none")}");

        SetState(GameState.Failed);
        StopInput();

        if (ballState != null)
            ballState.MarkFailed();

        if (failureHandler != null)
            failureHandler.PlayFailure(launcher, ballBody, at, reason, hitCollider);

        if (ui != null)
        {
            ui.HideEndScreen();
            ui.ShowFailure(onRetry: RetryLevel);
        }

        onFailure?.Invoke();
        return true;
    }
}
