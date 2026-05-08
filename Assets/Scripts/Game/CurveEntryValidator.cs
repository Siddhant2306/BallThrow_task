using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class CurveEntryValidator : MonoBehaviour
{
    [Header("Ball Filtering")]
    [SerializeField] private ProjectileLauncher ball;
    [SerializeField] private bool autoFindBall = true;
    [SerializeField] private bool requireLaunchedBall = true;

    [Header("Entry")]
    [Tooltip("Ball must enter this trigger before colliding with the curve surface.")]
    [SerializeField] private Collider entryTrigger;

    [Header("Failure")]
    [SerializeField] private FailureReason failureReason = FailureReason.MissedCurve;
    [SerializeField] private bool onlyFailOncePerAttempt = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool enteredThisAttempt;
    private bool failedThisAttempt;

    private void OnEnable()
    {
        ResolveBall();
        Subscribe();
        ResetAttempt();

        if (entryTrigger != null && entryTrigger.gameObject != gameObject && debugLogs)
            Debug.LogWarning("[BallThrow] CurveEntryValidator: entryTrigger should be on the SAME GameObject as this component.", this);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveBall()
    {
        if (ball != null || !autoFindBall)
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
        ResetAttempt();
    }

    private void ResetAttempt()
    {
        enteredThisAttempt = false;
        failedThisAttempt = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entryTrigger == null)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] CurveEntryValidator: entryTrigger not assigned.", this);
            return;
        }

        // NOTE: With multiple trigger colliders on the same GameObject, Unity doesn't tell us which
        // collider triggered the callback. Keep this component on a curve object that has only ONE
        // trigger used as the entry region (assigned as entryTrigger).

        if (!entryTrigger.isTrigger)
            return;

        Rigidbody body = other.attachedRigidbody;
        if (body == null)
            return;

        if (!body.TryGetComponent(out ProjectileLauncher launcher))
            return;

        if (ball == null)
        {
            ball = launcher;
            Subscribe();
        }

        if (launcher != ball)
            return;

        if (requireLaunchedBall && !launcher.HasLaunched)
            return;

        enteredThisAttempt = true;

        if (debugLogs)
            Debug.Log($"[BallThrow] CurveEntryValidator: entry OK for {launcher.name}", this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (onlyFailOncePerAttempt && failedThisAttempt)
            return;

        Rigidbody body = collision.rigidbody;
        if (body == null)
            return;

        if (!body.TryGetComponent(out ProjectileLauncher launcher))
            return;

        if (ball == null)
        {
            ball = launcher;
            Subscribe();
        }

        if (launcher != ball)
            return;

        if (requireLaunchedBall && !launcher.HasLaunched)
            return;

        if (enteredThisAttempt)
            return;

        failedThisAttempt = true;

        Vector3 at = collision.contactCount > 0 ? collision.GetContact(0).point : body.position;

        if (debugLogs)
            Debug.Log($"[BallThrow] CurveEntryValidator: MISSED entry -> Fail ({failureReason}) at {at}", this);

        if (GameManager.Instance != null)
            GameManager.Instance.Fail(failureReason, at, collision.collider);
    }
}
