using BallThrow.Gameplay;
using UnityEngine;

public partial class GameManager
{
    private float launchTime;
    private float restTimer;

    private void Update()
    {
        TickState();
    }

    private void SetState(GameState newState, bool force = false)
    {
        if (!force && State == newState)
            return;

        GameState previous = State;
        State = newState;

        Log($"State {previous} -> {State}");
        OnEnterState(State, previous);
    }

    private void TickState()
    {
        if (State == GameState.Success || State == GameState.Failed)
            return;

        switch (State)
        {
            case GameState.Idle:
                TickIdle();
                break;
            case GameState.Aiming:
                TickAiming();
                break;
            case GameState.Flying:
                TickFlying();
                break;
        }
    }

    private void OnEnterState(GameState entered, GameState previous)
    {
        switch (entered)
        {
            case GameState.Flying:
                EnterFlying(previous);
                break;
        }
    }

    private void TickIdle()
    {
        if (launcher == null)
            return;

        if (!launcher.HasLaunched && launcher.IsDragging)
        {
            Log("Idle -> Aiming (drag started)");
            SetState(GameState.Aiming);
            return;
        }

        if (launcher.HasLaunched)
        {
            Log("Idle -> Flying (already launched)");
            SetState(GameState.Flying);
        }
    }

    private void TickAiming()
    {
        if (launcher == null)
            return;

        if (launcher.HasLaunched)
        {
            Log("Aiming -> Flying (launched)");
            SetState(GameState.Flying);
            return;
        }

        if (!launcher.IsDragging)
        {
            Log("Aiming -> Idle (drag canceled)");
            SetState(GameState.Idle);
        }
    }

    private void EnterFlying(GameState previous)
    {
        VLog($"EnterFlying (from {previous})");
        launchTime = Time.time;
        restTimer = 0f;
    }

    private void TickFlying()
    {
        if (ballBody == null)
            return;

        if (Time.time - launchTime > maxFlightTime)
        {
            Log("Flying failure: Timeout");
            Fail(FailureReason.Timeout, ballBody.position);
            return;
        }

        if (ballBody.position.y < minYBeforeFail)
        {
            Log("Flying failure: FailZone (minY)");
            Fail(FailureReason.FailZone, ballBody.position);
            return;
        }

        if (failIfOutOfBounds)
        {
            Vector3 p = ballBody.position;
            if (p.x < xBounds.x || p.x > xBounds.y || p.y < yBounds.x || p.y > yBounds.y || Mathf.Abs(p.z) > maxAbsZ)
            {
                Log("Flying failure: OutOfBounds");
                Fail(FailureReason.OutOfBounds, p);
                return;
            }
        }

        float speed = ballBody.linearVelocity.magnitude;
        if (speed <= restSpeed)
            restTimer += Time.deltaTime;
        else
            restTimer = 0f;

        if (restTimer >= restTime)
        {
            Log("Flying failure: TooSlow (rest)");
            Fail(FailureReason.TooSlow, ballBody.position);
        }
    }

    private void HandleLaunched(Vector3 velocity)
    {
        Log($"Launcher event: Launched velocity={velocity}");

        if (State is GameState.Success or GameState.Failed)
            return;

        SetState(GameState.Flying);
    }

    private void HandleReset()
    {
        Log("Launcher event: Reset");

        RestoreTimeScale();

        if (stopInputByDisablingLauncher && launcher != null)
            launcher.enabled = true;

        if (ballState != null)
            ballState.enabled = true;

        if (ui != null)
            ui.HideEndScreen();

        SetState(GameState.Idle, force: true);
    }

    private void StopInput()
    {
        if (stopInputByDisablingLauncher && launcher != null)
            launcher.enabled = false;
    }
}
