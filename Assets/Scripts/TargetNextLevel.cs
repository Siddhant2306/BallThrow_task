using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TargetNextLevel : MonoBehaviour
{
    [Header("Filtering")]
    [SerializeField] private ProjectileLauncher ball;
    [SerializeField] private float minSuccessSpeed = 2f;
    [SerializeField] private bool requireLaunchedBall = true;
    [SerializeField] private bool onlyOncePerAttempt = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogOnSuccess = true;

    [Header("Events")]
    public UnityEvent onTargetHit = new UnityEvent();

    private bool hasTriggered;

    private void OnEnable()
    {
        ResolveBall();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveBall()
    {
        if (ball != null)
            return;

        ball = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);
    }

    private void Subscribe()
    {
        if (ball == null)
            return;

        ball.Reset -= HandleBallReset;
        ball.Reset += HandleBallReset;
    }

    private void Unsubscribe()
    {
        if (ball == null)
            return;

        ball.Reset -= HandleBallReset;
    }

    private void HandleBallReset()
    {
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTrigger(other.attachedRigidbody);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTrigger(collision.rigidbody);
    }

    private void TryTrigger(Rigidbody otherBody)
    {
        if (onlyOncePerAttempt && hasTriggered)
            return;

        if (otherBody == null)
            return;

        if (!otherBody.TryGetComponent(out ProjectileLauncher hitBall))
            return;

        if (ball == null)
        {
            ball = hitBall;
            Subscribe();
        }

        if (ball != null && hitBall != ball)
            return;

        if (requireLaunchedBall && !hitBall.HasLaunched)
            return;

        hasTriggered = true;

        if (debugLogOnSuccess)
            Debug.Log("Target hit! Attempting success.", this);

        bool successAccepted = false;

        if (GameManager.Instance != null)
        {
            successAccepted = GameManager.Instance.TrySuccess(otherBody, transform, minSuccessSpeed);
            if (debugLogOnSuccess)
                Debug.Log($"Target hit: TrySuccess returned {successAccepted}.", this);
        }
        else if (debugLogOnSuccess)
        {
            Debug.LogWarning("Target hit, but no GameManager.Instance found (success not applied).", this);
        }

        if (successAccepted)
        {
            GoalPulseOnSuccess pulse =
                GetComponentInParent<GoalPulseOnSuccess>(includeInactive: true) ??
                GetComponentInChildren<GoalPulseOnSuccess>(includeInactive: true);

            if (pulse != null)
                pulse.PlayPulse();

            onTargetHit?.Invoke();
        }
    }
}
