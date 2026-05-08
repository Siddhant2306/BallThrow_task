using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class BallStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private Rigidbody body;
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private TrailRenderer trail;

    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color successColor = new Color(0.2f, 1f, 0.6f, 1f);
    [SerializeField] private Color failedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public BallState State { get; private set; } = BallState.Ready;

    private MaterialPropertyBlock mpb;
    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        if (launcher == null)
            launcher = GetComponent<ProjectileLauncher>();

        if (body == null)
            body = GetComponent<Rigidbody>();

        if (ballRenderer == null)
            ballRenderer = GetComponentInChildren<Renderer>(includeInactive: true);

        if (trail == null)
            trail = GetComponentInChildren<TrailRenderer>(includeInactive: true);

        mpb = new MaterialPropertyBlock();
        ApplyColor(readyColor);
        SetTrail(false);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (launcher == null)
            return;

        launcher.Launched -= HandleLaunched;
        launcher.Launched += HandleLaunched;

        launcher.Reset -= HandleReset;
        launcher.Reset += HandleReset;
    }

    private void Unsubscribe()
    {
        if (launcher == null)
            return;

        launcher.Launched -= HandleLaunched;
        launcher.Reset -= HandleReset;
    }

    private void HandleLaunched(Vector3 _)
    {
        if (debugLogs)
            Debug.Log("[BallThrow] BallStateController: Launched", this);

        State = BallState.Launched;
        SetTrail(true);
    }

    private void HandleReset()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] BallStateController: Reset", this);

        State = BallState.Ready;
        ApplyColor(readyColor);
        SetTrail(false);
    }

    public void MarkSuccess()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] BallStateController: MarkSuccess", this);

        State = BallState.Success;
        ApplyColor(successColor);
        // Trail should only ever be enabled by the launch event.
    }

    public void MarkFailed()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] BallStateController: MarkFailed", this);

        State = BallState.Failed;
        ApplyColor(failedColor);
        SetTrail(false);
    }

    private void ApplyColor(Color c)
    {
        if (ballRenderer == null)
            return;

        ballRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(ColorId, c);
        ballRenderer.SetPropertyBlock(mpb);
    }

    private void SetTrail(bool enabled)
    {
        if (trail == null)
            return;

        trail.emitting = enabled;
        if (!enabled)
            trail.Clear();
    }
}

