using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class BallStateController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private TrailRenderer trail;

    [Header("Failure Dissolve (optional)")]
    [Tooltip("Plays a dissolve on failure using a material float property on the ball's shader.")]
    [SerializeField] private bool dissolveOnFail = true;
    [Tooltip("Float property name on the ball material (e.g. _DissolveAmount, _Cutoff, _Dissolve).")]
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float dissolveDuration = 0.35f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color successColor = new Color(0.2f, 1f, 0.6f, 1f);
    [SerializeField] private Color failedColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public BallState State { get; private set; } = BallState.Ready;

    private MaterialPropertyBlock mpb;
    private static readonly int ColorId = Shader.PropertyToID("_BaseColor");
    private int dissolveId;
    private Coroutine dissolveRoutine;

    private void Awake()
    {
        if (launcher == null)
            launcher = GetComponent<ProjectileLauncher>();

        if (ballRenderer == null)
            ballRenderer = GetComponentInChildren<Renderer>(includeInactive: true);

        if (trail == null)
            trail = GetComponentInChildren<TrailRenderer>(includeInactive: true);

        mpb = new MaterialPropertyBlock();
        dissolveId = !string.IsNullOrWhiteSpace(dissolveProperty) ? Shader.PropertyToID(dissolveProperty) : 0;
        ApplyColor(readyColor);
        SetDissolve01(0f);
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

        if (dissolveRoutine != null)
        {
            StopCoroutine(dissolveRoutine);
            dissolveRoutine = null;
        }

        State = BallState.Ready;
        ApplyColor(readyColor);
        SetDissolve01(0f);
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

        if (dissolveOnFail && ballRenderer != null && dissolveId != 0)
        {
            if (dissolveRoutine != null)
                StopCoroutine(dissolveRoutine);

            dissolveRoutine = StartCoroutine(DissolveRoutine());
        }
    }

    private void ApplyColor(Color c)
    {
        if (ballRenderer == null)
            return;

        ballRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(ColorId, c);
        ballRenderer.SetPropertyBlock(mpb);
    }

    private void SetDissolve01(float value01)
    {
        if (ballRenderer == null || dissolveId == 0)
            return;

        ballRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(dissolveId, Mathf.Clamp01(value01));
        ballRenderer.SetPropertyBlock(mpb);
    }

    private System.Collections.IEnumerator DissolveRoutine()
    {
        if (debugLogs)
            Debug.Log($"[BallThrow] BallStateController: Dissolve start (property='{dissolveProperty}', duration={dissolveDuration:0.00})", this);

        float duration = Mathf.Max(0.01f, dissolveDuration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            float eased = dissolveCurve != null ? dissolveCurve.Evaluate(u) : u;
            SetDissolve01(eased);
            yield return null;
        }

        SetDissolve01(1f);
        dissolveRoutine = null;

        if (debugLogs)
            Debug.Log("[BallThrow] BallStateController: Dissolve end", this);
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

