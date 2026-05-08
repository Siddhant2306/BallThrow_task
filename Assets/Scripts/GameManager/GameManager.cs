using BallThrow.Gameplay;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public partial class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private Rigidbody ballBody;
    [SerializeField] private BallStateController ballState;
    [SerializeField] private SuccessHandler successHandler;
    [SerializeField] private FailureHandler failureHandler;
    [SerializeField] private UIController ui;
    [SerializeField] private FeedbackManager feedback;

    [Header("Level Progress")]
    [Tooltip("Set per-level (1-based). Used for PlayerPrefs unlocks.")]
    [SerializeField] private int levelIndex = 1;
    [SerializeField] private bool unlockNextLevelOnSuccess = true;
    [SerializeField] private string levelsUnlockedKey = "levels_unlocked";

    [Header("State")]
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private bool stopInputByDisablingLauncher = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool verboseLogs = false;

    [Header("Performance")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private bool disableVSync = true;

    [Header("Failure Conditions (Manager)")]
    [SerializeField] private float minYBeforeFail = -8f;
    [SerializeField] private float maxFlightTime = 8f;
    [SerializeField] private float restSpeed = 0.25f;
    [SerializeField] private float restTime = 0.9f;
    [SerializeField] private bool failIfOutOfBounds = true;
    [SerializeField] private Vector2 xBounds = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 yBounds = new Vector2(-20f, 200f);
    [SerializeField] private float maxAbsZ = 20f;

    [Header("Events")]
    public UnityEvent<GameState> onGameStateChanged = new UnityEvent<GameState>();
    public UnityEvent onSuccess = new UnityEvent();
    public UnityEvent onFailure = new UnityEvent();

    public GameState State { get; private set; } = GameState.Idle;
    public ProjectileLauncher Launcher => launcher;

    private float defaultFixedDeltaTime;

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log($"[BallThrow] {message}", this);
    }

    private void VLog(string message)
    {
        if (!debugLogs || !verboseLogs)
            return;

        Debug.Log($"[BallThrow][Verbose] {message}", this);
    }

    private void Warn(string message)
    {
        if (!debugLogs)
            return;

        Debug.LogWarning($"[BallThrow] {message}", this);
    }
}
