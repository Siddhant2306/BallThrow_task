namespace BallThrow.Gameplay
{
    public enum GameState
    {
        Idle,
        Aiming,
        Flying,
        Success,
        Failed,
    }

    public enum BallState
    {
        Ready,
        Launched,
        Impact,
        Sliding,
        Success,
        Failed,
    }

    public enum FailureReason
    {
        FailZone,
        HardImpact,
        MissedCurve,
        TooSlow,
        OutOfBounds,
        Timeout,
        Custom,
    }
}

